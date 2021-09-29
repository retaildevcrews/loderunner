// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using LodeRunner.API;
using LodeRunner.API.Middleware.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.CorrelationVector;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prometheus;

namespace LodeRunner.API.Middleware
{
    /// <summary>
    /// Simple aspnet core middleware that logs requests to the console
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
        /// <param name="next">RequestDelegate</param>
        /// <param name="options">LoggerOptions</param>
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
                "RelayRunnerDuration",
                "Histogram of RelayRunner request duration",
                new HistogramConfiguration
                {
                    Buckets = Histogram.ExponentialBuckets(1, 2, 10),
                    LabelNames = new string[] { "code", "mode" },
                });

            requestSummary = Metrics.CreateSummary(
                "RelayRunnerSummary",
                "Summary of RelayRunner request duration",
                new SummaryConfiguration
                {
                    SuppressInitialValue = true,
                    MaxAge = TimeSpan.FromMinutes(5),
                    Objectives = new List<QuantileEpsilonPair> { new QuantileEpsilonPair(.9, .0), new QuantileEpsilonPair(.95, .0), new QuantileEpsilonPair(.99, .0), new QuantileEpsilonPair(1.0, .0) },
                    LabelNames = new string[] { "code", "mode" },
                });
        }

        public static string CosmosName { get; set; } = string.Empty;
        public static string CosmosQueryId { get; set; } = string.Empty;
        public static double CosmosRUs { get; set; } = 0;
        public static string Zone { get; set; } = string.Empty;
        public static string Region { get; set; } = string.Empty;

        /// <summary>
        /// Return the path and query string if it exists
        /// </summary>
        /// <param name="request">HttpRequest</param>
        /// <returns>string</returns>
        public static string GetPathAndQuerystring(HttpRequest request)
        {
            if (request == null || !request.Path.HasValue)
            {
                return string.Empty;
            }

            return HttpUtility.UrlDecode(HttpUtility.UrlEncode(request.Path.Value + (request.QueryString.HasValue ? request.QueryString.Value : string.Empty)));
        }

        /// <summary>
        /// Called by aspnet pipeline
        /// </summary>
        /// <param name="context">HttpContext</param>
        /// <returns>Task (void)</returns>
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

            // Invoke next handler
            if (next != null)
            {
                await next.Invoke(context).ConfigureAwait(false);
            }

            duration = Math.Round(DateTime.Now.Subtract(dtStart).TotalMilliseconds, 2);
            ttfb = ttfb == 0 ? duration : ttfb;

            await context.Response.CompleteAsync();

            // compute request duration
            duration = Math.Round(DateTime.Now.Subtract(dtStart).TotalMilliseconds, 2);

            LogRequest(context, cv, ttfb, duration);
        }

        // convert StatusCode for metrics
        private static string GetPrometheusCode(int statusCode)
        {
            if (statusCode >= 500)
            {
                return "Error";
            }
            else if (statusCode == 429)
            {
                return "Retry";
            }
            else if (statusCode >= 400)
            {
                return "Warn";
            }
            else
            {
                return "OK";
            }
        }

        // get the client IP address from the request / headers
        private static string GetClientIp(HttpContext context, out string xff)
        {
            const string XffHeader = "X-Forwarded-For";
            const string IpHeader = "X-Client-IP";

            xff = string.Empty;
            string clientIp = context.Connection.RemoteIpAddress.ToString();

            // check for the forwarded headers
            if (context.Request.Headers.ContainsKey(XffHeader))
            {
                xff = context.Request.Headers[XffHeader].ToString().Trim();

                // add the clientIp to the list of proxies
                xff += $", {clientIp}";

                // get the first IP in the xff header (comma space separated)
                string[] ips = xff.Split(',');

                if (ips.Length > 0)
                {
                    clientIp = ips[0].Trim();
                }
            }
            else if (context.Request.Headers.ContainsKey(IpHeader))
            {
                // fall back to X-Client-IP if xff not set
                xff = context.Request.Headers[IpHeader].ToString().Trim();
                clientIp = xff;
            }

            // remove IP6 local address
            return clientIp.Replace("::ffff:", string.Empty);
        }

        // log the request
        private void LogRequest(HttpContext context, CorrelationVector cv, double ttfb, double duration)
        {
            DateTime dt = DateTime.UtcNow;

            string category = ValidationError.GetCategory(context, out string subCategory, out string mode);

            if (config.RequestLogLevel != LogLevel.None &&
                (config.RequestLogLevel <= LogLevel.Information ||
                (config.RequestLogLevel == LogLevel.Warning && context.Response.StatusCode >= 400) ||
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
                    { "Subcategory", subCategory },
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
