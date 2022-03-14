// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using LodeRunner.API.Middleware.Validation;
using LodeRunner.Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.CorrelationVector;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prometheus;
using CorrelationVectorExtensions = LodeRunner.Core.Extensions.CorrelationVectorExtensions;

namespace LodeRunner.API.Middleware
{
    /// <summary>
    /// Simple aspnet core middleware that logs requests to the console.
    /// </summary>
    public class RequestLogger
    {
        private static Histogram requestHistogram = null;
        private static Summary requestSummary = null;

        // next action to Invoke
        private readonly RequestDelegate next;
        private readonly RequestLoggerOptions options;

        private readonly Config config;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestLogger"/> class.
        /// </summary>
        /// <param name="next">RequestDelegate.</param>
        /// <param name="options">LoggerOptions.</param>
        /// <param name="config">App configuration object.</param>
        public RequestLogger(RequestDelegate next, IOptions<RequestLoggerOptions> options, Config config)
        {
            this.config = config;

            // save for later
            this.next = next;
            this.options = options?.Value;

            if (this.options == null)
            {
                // use default
                this.options = new RequestLoggerOptions();
            }

            requestHistogram = Metrics.CreateHistogram(
                "LodeRunnerAPIDuration",
                "Histogram of LodeRunnerAPI request duration",
                new HistogramConfiguration
                {
                    Buckets = Histogram.ExponentialBuckets(1, 2, 10),
                    LabelNames = new string[] { "code", "mode" },
                });

            requestSummary = Metrics.CreateSummary(
                "LodeRunnerAPISummary",
                "Summary of LodeRunnerAPI request duration",
                new SummaryConfiguration
                {
                    SuppressInitialValue = true,
                    MaxAge = TimeSpan.FromMinutes(5),
                    Objectives = new List<QuantileEpsilonPair> { new QuantileEpsilonPair(.9, .0), new QuantileEpsilonPair(.95, .0), new QuantileEpsilonPair(.99, .0), new QuantileEpsilonPair(1.0, .0) },
                    LabelNames = new string[] { "code", "mode" },
                });
        }

        /// <summary>
        /// Gets or sets CosmosDB name.
        /// </summary>
        public static string CosmosName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets Cosmos query ID.
        /// </summary>
        public static string CosmosQueryId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets Cosmos RUs.
        /// </summary>
        public static double CosmosRUs { get; set; } = 0;

        /// <summary>
        /// Gets or sets the zone.
        /// </summary>
        public static string Zone { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the region.
        /// </summary>
        public static string Region { get; set; } = string.Empty;

        /// <summary>
        /// Return the path and query string if it exists.
        /// </summary>
        /// <param name="request">HttpRequest.</param>
        /// <returns>string.</returns>
        public static string GetPathAndQuerystring(HttpRequest request)
        {
            if (request == null || !request.Path.HasValue)
            {
                return string.Empty;
            }

            return HttpUtility.UrlDecode(HttpUtility.UrlEncode(request.Path.Value + (request.QueryString.HasValue ? request.QueryString.Value : string.Empty)));
        }

        /// <summary>
        /// Called by aspnet pipeline.
        /// </summary>
        /// <param name="context">HttpContext.</param>
        /// <returns>Task (void).</returns>
        public async Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                return;
            }

            // don't log favicon.ico 404s
            if (context.Request.Path.StartsWithSegments("/favicon.ico", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = 404;
                return;
            }

            DateTime dtStart = DateTime.Now;
            double duration = 0;
            double ttfb = 0;

            CorrelationVector cv = CorrelationVectorExtensions.Extend(context);

            // Buffering needs to be enabled to allow payload to be read from request body before next is called.
            var payload = GetRequestPayloadObject(context.Request);

            // Invoke next handler
            if (this.next != null)
            {
                await this.next.Invoke(context).ConfigureAwait(false);
            }

            duration = Math.Round(DateTime.Now.Subtract(dtStart).TotalMilliseconds, 2);
            ttfb = ttfb == 0 ? duration : ttfb;

            await context.Response.CompleteAsync();

            // compute request duration
            duration = Math.Round(DateTime.Now.Subtract(dtStart).TotalMilliseconds, 2);

            this.LogRequest(context, cv, ttfb, duration, payload);
        }

        // convert StatusCode for metrics
        private static string GetPrometheusCode(int statusCode)
        {
            if (statusCode >= (int)HttpStatusCode.InternalServerError)
            {
                return SystemConstants.Error;
            }
            else if (statusCode == (int)HttpStatusCode.TooManyRequests)
            {
                return SystemConstants.Retry;
            }
            else if (statusCode >= (int)HttpStatusCode.BadRequest)
            {
                return SystemConstants.Warn;
            }
            else
            {
                return SystemConstants.OK;
            }
        }

        // get the client IP address from the request / headers
        private static string GetClientIp(HttpContext context, out string xff)
        {
            // Note: context.Connection.RemoteIpAddress will be null when running Test Project
            xff = string.Empty;
            string clientIp = context.Connection.RemoteIpAddress?.ToString();

            // check for the forwarded headers
            if (context.Request.Headers.ContainsKey(SystemConstants.XffHeader))
            {
                xff = context.Request.Headers[SystemConstants.XffHeader].ToString().Trim();

                // add the clientIp to the list of proxies
                xff += $", {clientIp}";

                // get the first IP in the xff header (comma space separated)
                string[] ips = xff.Split(',');

                if (ips.Length > 0)
                {
                    clientIp = ips[0].Trim();
                }
            }
            else if (context.Request.Headers.ContainsKey(SystemConstants.IpHeader))
            {
                // fall back to X-Client-IP if xff not set
                xff = context.Request.Headers[SystemConstants.IpHeader].ToString().Trim();
                clientIp = xff;
            }

            // Note: clientIp will be null when running Test Project
            // remove IP6 local address
            return clientIp?.Replace("::ffff:", string.Empty);
        }

        /// <summary>
        /// Extracts and returns request body
        /// </summary>
        /// <param name="request">HttpRequest</param>
        /// <returns>Request body as object.</returns>
        private static object GetRequestPayloadObject(HttpRequest request)
        {
            if (request.Method == "POST" || request.Method == "PUT")
            {
                // Allows using the stream several times in ASP.Net Core
                request.EnableBuffering();

                string requestBodyString;
                using (StreamReader reader = new (request.Body, leaveOpen: true))
                {
                    requestBodyString = reader.ReadToEndAsync().Result;
                    request.Body.Position = 0;
                }

                return JsonSerializer.Deserialize<object>(requestBodyString);
            }

            return null;
        }

        // log the request
        private void LogRequest(HttpContext context, CorrelationVector cv, double ttfb, double duration, object payload)
        {
            DateTime dt = DateTime.UtcNow;

            string category = ValidationError.GetCategory(context, out string mode);

            if (this.config.RequestLogLevel != LogLevel.None &&
                (this.config.RequestLogLevel <= LogLevel.Information ||
                (this.config.RequestLogLevel == LogLevel.Warning && context.Response.StatusCode >= 400) ||
                context.Response.StatusCode >= 500))
            {
                Dictionary<string, object> log = new ()
                {
                    { "Date", dt },
                    { "LogName", "LodeRunner.API.RequestLog" },
                    { "StatusCode", context.Response.StatusCode },
                    { "TTFB", ttfb },
                    { "Duration", duration },
                    { "Verb", context.Request.Method },
                    { "Path", GetPathAndQuerystring(context.Request) },
                    { "Host", context.Request.Headers["Host"].ToString() },
                    { "ClientIP", GetClientIp(context, out string xff) },
                    { "XFF", xff },
                    { "UserAgent", context.Request.Headers["User-Agent"].ToString() },
                    { "CVector", cv.Value },
                    { "CVectorBase", cv.GetBase() },
                    { "Category", category },
                    { "Mode", mode },
                };

                if (!string.IsNullOrWhiteSpace(Zone))
                {
                    log.Add("Zone", Zone);
                }

                if (!string.IsNullOrWhiteSpace(Region))
                {
                    log.Add("Region", Region);
                }

                if (!string.IsNullOrWhiteSpace(CosmosName))
                {
                    log.Add("CosmosName", CosmosName);
                }

                if (!string.IsNullOrWhiteSpace(CosmosQueryId))
                {
                    log.Add("CosmosQueryId", CosmosQueryId);
                }

                if (CosmosRUs > 0)
                {
                    log.Add("CosmosRUs", CosmosRUs);
                }

                if (payload != null)
                {
                    log.Add("Payload", payload);
                }

                // write the results to the console
                Console.WriteLine(JsonSerializer.Serialize(log));
            }

            if (requestHistogram != null && (mode == "Direct" || mode == "Static"))
            {
                requestHistogram.WithLabels(GetPrometheusCode(context.Response.StatusCode), mode).Observe(duration);
                requestSummary.WithLabels(GetPrometheusCode(context.Response.StatusCode), mode).Observe(duration);
            }
        }
    }
}
