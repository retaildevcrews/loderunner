// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.Core.Events;
using LodeRunner.Core.NgsaLogger;
using LodeRunner.Model;
using LodeRunner.Validators;
using Microsoft.CorrelationVector;
using Microsoft.Extensions.Logging;
using Ngsa.Middleware;
using Prometheus;

namespace LodeRunner
{
    /// <summary>
    /// LodeRunner Test.
    /// </summary>
    public partial class ValidationTest
    {
        /// <summary>
        /// Correlation Vector http header name.
        /// </summary>
        private static readonly JsonSerializerOptions JsonOptions = new ()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        private static Histogram requestDuration = null;
        private static Summary requestSummary = null;
        private static List<Request> requestList;
        private static SocketsHttpHandler httpSocketHandler;

        private readonly Dictionary<string, PerfTarget> targets = new ();

        private readonly ILogger logger;

        private Config config;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationTest"/> class.
        /// Constructor for ValidationTest.
        /// </summary>
        /// <param name="config">app config.</param>
        /// <param name="logger">the logger.</param>
        public ValidationTest(Config config, ILogger logger)
        {
            if (config == null || config.Files == null || config.Server == null || config.Server.Count == 0)
            {
                throw new ArgumentNullException(nameof(config));
            }

            this.logger = logger;

            this.config = config;

            // load the performance targets
            this.targets = this.LoadPerfTargets();

            // load the requests from json files
            requestList = this.LoadValidateRequests(config.Files);

            if (requestList == null || requestList.Count == 0)
            {
                throw new ArgumentException("RequestList is empty");
            }

            // use socketshttphandler to avoid stale DNS and socket exhaustion problems
            httpSocketHandler = new ()
            {
                AllowAutoRedirect = false,
            };

            if (config.ClientRefresh > 0)
            {
                httpSocketHandler.PooledConnectionLifetime = TimeSpan.FromSeconds(config.ClientRefresh);
            }
        }

        /// <summary>
        /// Gets UtcNow as an ISO formatted date string.
        /// </summary>
        public static string Now => DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);

        /// <summary>
        /// Gets Histogram.
        /// </summary>
        public Histogram RequestDuration
        {
            get
            {
                if (this.config.Prometheus && requestDuration == null)
                {
                    requestDuration = Metrics.CreateHistogram(
                    "LodeRunnerDuration",
                    "Histogram of LodeRunner request duration",
                    new HistogramConfiguration
                    {
                        Buckets = Histogram.ExponentialBuckets(1, 2, 10),
                        LabelNames = new string[] { "code", "mode", "server", "failed", "zone", "region" },
                    });
                }

                return requestDuration;
            }
        }

        /// <summary>
        /// Gets Summary.
        /// </summary>
        public Summary RequestSummary
        {
            get
            {
                if (this.config.Prometheus && requestSummary == null)
                {
                    requestSummary = Metrics.CreateSummary(
                        "LodeRunnerSummary",
                        "Summary of LodeRunner request duration",
                        new SummaryConfiguration
                        {
                            SuppressInitialValue = true,
                            MaxAge = TimeSpan.FromMinutes(5),
                            Objectives = new List<QuantileEpsilonPair> { new QuantileEpsilonPair(.9, .0), new QuantileEpsilonPair(.95, .0), new QuantileEpsilonPair(.99, .0), new QuantileEpsilonPair(1.0, .0) },
                            LabelNames = new string[] { "code", "mode", "server", "failed", "zone", "region" },
                        });
                }

                return requestSummary;
            }
        }

        /// <summary>
        /// Run the validation test one time.
        /// </summary>
        /// <param name="config">configuration.</param>
        /// <param name="token">cancellation token.</param>
        /// <returns>bool.</returns>
        public async Task<int> RunOnce(Config config, CancellationToken token)
        {
            if (config == null)
            {
                Console.WriteLine("RunOnce:Config is null");
                return Core.SystemConstants.ExitFail;
            }

            int duration;
            PerfLog pl;
            int errorCount = 0;
            int validationFailureCount = 0;
            int requestCount = 0;

            DateTime startTime = DateTime.UtcNow;

            // loop through each server
            for (int ndx = 0; ndx < config.Server.Count; ndx++)
            {
                // reset error counts
                if (config.Server.Count > 0)
                {
                    if (ndx > 0)
                    {
                        // Note: We are separating console output.
                        Console.WriteLine();
                        errorCount = 0;
                        validationFailureCount = 0;
                    }
                }

                using HttpClient client = this.OpenClient(ndx);

                // send each request
                foreach (Request r in requestList)
                {
                    try
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }

                        // stop after MaxErrors errors
                        if ((errorCount + validationFailureCount) >= config.MaxErrors)
                        {
                            break;
                        }

                        // execute the request
                        pl = await this.ExecuteRequest(client, config.Server[ndx], r).ConfigureAwait(false);

                        requestCount++;

                        if (pl.Failed)
                        {
                            errorCount++;
                        }

                        if (!pl.Failed && !pl.Validated)
                        {
                            validationFailureCount++;
                        }

                        // sleep if configured
                        if (config.Sleep > 0)
                        {
                            duration = config.Sleep - (int)pl.Duration;

                            if (duration > 0)
                            {
                                await Task.Delay(duration, token).ConfigureAwait(false);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // ignore any exception caused by ctl-c or stop signal
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }

                        // log error and keep processing
                        this.logger.LogError(new EventId((int)EventTypes.CommonEvents.Exception, nameof(RunOnce)), ex, "Exception");

                        errorCount++;
                    }
                }

                // log validation failure count
                if (validationFailureCount > 0)
                {
                    Console.WriteLine($"Validation Errors: {validationFailureCount}");
                }

                // log error count
                if (errorCount > 0)
                {
                    Console.WriteLine($"Failed: {errorCount} Errors");
                }

                // log MaxErrors exceeded
                if (errorCount + validationFailureCount >= config.MaxErrors)
                {
                    Console.Write($"Failed: Errors: {errorCount + validationFailureCount} >= MaxErrors: {config.MaxErrors}");
                }
            }

            // fire event
            TestRunComplete(null, new LoadResultEventArgs(startTime, DateTime.UtcNow, config.TestRunId, requestCount, validationFailureCount + errorCount));

            // return non-zero exit code on failure
            return errorCount > 0 || validationFailureCount >= config.MaxErrors ? errorCount + validationFailureCount : 0;
        }

        /// <summary>
        /// Run the validation tests in a loop.
        /// </summary>
        /// <param name="config">Config.</param>
        /// <param name="token">CancellationToken.</param>
        /// <returns>0 on success.</returns>
        /// <returns>1 on failure.</returns>
        public int RunLoop(Config config, CancellationToken token)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));

            DateTime dtMax = DateTime.MaxValue;

            DateTime startTime = DateTime.UtcNow;

            // only run for duration (seconds)
            if (config.Duration > 0)
            {
                dtMax = DateTime.UtcNow.AddSeconds(config.Duration);
            }

            if (config.Sleep < 1)
            {
                config.Sleep = 1;
            }

            DisplayStartupMessage(config);

            List<TimerRequestState> states = new ();

            foreach (string svr in config.Server)
            {
                // create the shared state
                TimerRequestState state = new (this.logger)
                {
                    Server = svr,
                    Client = this.OpenHttpClient(svr),
                    MaxIndex = requestList.Count,
                    Test = this,
                    RequestList = requestList,

                    // current hour
                    CurrentLogTime = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, 0, 0),

                    Token = token,
                };

                state.Random = config.Random;

                states.Add(state);

                state.Run(config.Sleep, config.MaxConcurrent);
            }

            try
            {
                // run the wait loop
                if (dtMax == DateTime.MaxValue)
                {
                    Task.Delay(-1, token).Wait(token);
                }
                else
                {
                    // wait one hour to keep total milliseconds from overflowing
                    while (dtMax.Subtract(DateTime.UtcNow).TotalHours > 1)
                    {
                        Task.Delay(60 * 60 * 1000, token).Wait(token);
                    }

                    int delay = (int)dtMax.Subtract(DateTime.UtcNow).TotalMilliseconds;

                    if (delay > 0)
                    {
                        Task.Delay(delay, token).Wait(token);
                    }
                }
            }
            catch (TaskCanceledException tce)
            {
                TestRunComplete(null, new LoadResultEventArgs(startTime, DateTime.UtcNow, config.TestRunId, 0, 0, tce.Message));

                // log exception
                if (!tce.Task.IsCompleted)
                {
                    this.logger.LogError(new EventId((int)EventTypes.CommonEvents.Exception, nameof(RunLoop)), tce, "TaskCanceledException");

                    return Core.SystemConstants.ExitFail;
                }

                // task is completed
                return Core.SystemConstants.ExitSuccess;
            }
            catch (OperationCanceledException oce)
            {
                TestRunComplete(null, new LoadResultEventArgs(startTime, DateTime.UtcNow, config.TestRunId, 0, 0, oce.Message));

                // log exception
                if (!token.IsCancellationRequested)
                {
                    this.logger.LogError(new EventId((int)EventTypes.CommonEvents.Exception, nameof(RunLoop)), oce, "OperationCanceledException");

                    return Core.SystemConstants.ExitFail;
                }

                // Operation was cancelled
                return Core.SystemConstants.ExitSuccess;
            }
            catch (Exception ex)
            {
                TestRunComplete(null, new LoadResultEventArgs(startTime, DateTime.UtcNow, config.TestRunId, 0, 0, ex.Message));
                this.logger.LogError(new EventId((int)EventTypes.CommonEvents.Exception, nameof(RunLoop)), ex, "Exception");

                return Core.SystemConstants.ExitFail;
            }

            DateTime completedTime = DateTime.UtcNow;

            long totalRequests = 0;
            int totalFailures = 0;
            foreach (var state in states)
            {
                totalFailures += state.ErrorCount;
                totalRequests += state.Count;
            }

            // fire event
            TestRunComplete(null, new LoadResultEventArgs(startTime, completedTime, config.TestRunId, (int)totalRequests, totalFailures));

            // graceful exit
            return Core.SystemConstants.ExitSuccess;
        }

        /// <summary>
        /// Execute a single validation test.
        /// </summary>
        /// <param name="client">http client.</param>
        /// <param name="server">server URL.</param>
        /// <param name="request">Request.</param>
        /// <returns>PerfLog.</returns>
        public async Task<PerfLog> ExecuteRequest(HttpClient client, string server, Request request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            PerfLog perfLog;
            ValidationResult valid;

            // send the request
            using (HttpRequestMessage req = new (new HttpMethod(request.Verb), request.Path))
            {
                DateTime dt = DateTime.UtcNow;

                // add the headers to the http request
                if (request.Headers != null && request.Headers.Count > 0)
                {
                    foreach (string key in request.Headers.Keys)
                    {
                        req.Headers.Add(key, request.Headers[key]);
                    }
                }

                // create correlation vector and add to headers
                CorrelationVector cv = new (CorrelationVectorVersion.V2);
                req.Headers.Add(CorrelationVector.HeaderName, cv.Value);

                // add the body to the http request
                if (!string.IsNullOrWhiteSpace(request.Body))
                {
                    if (!string.IsNullOrWhiteSpace(request.ContentMediaType))
                    {
                        req.Content = new StringContent(request.Body, Encoding.UTF8, request.ContentMediaType);
                    }
                    else
                    {
                        req.Content = new StringContent(request.Body);
                    }
                }

                try
                {
                    // process the response
                    using HttpResponseMessage resp = await client.SendAsync(req).ConfigureAwait(false);
                    string body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);

                    double duration = DateTime.UtcNow.Subtract(dt).TotalMilliseconds;

                    // validate the response
                    valid = ResponseValidator.Validate(request, resp, body);

                    // check the performance
                    perfLog = this.CreatePerfLog(server, request, valid, duration, (long)resp.Content.Headers.ContentLength, (int)resp.StatusCode);

                    // add correlation vector to perf log
                    perfLog.CorrelationVector = cv.Value;
                    perfLog.CorrelationVectorBase = cv.GetBase();
                }
                catch (Exception ex)
                {
                    double duration = Math.Round(DateTime.UtcNow.Subtract(dt).TotalMilliseconds, 0);
                    valid = new ValidationResult { Failed = true };
                    valid.ValidationErrors.Add($"Exception: {ex.Message}");
                    perfLog = this.CreatePerfLog(server, request, valid, duration, 0, 500);
                }
            }

            // log the test
            this.LogToConsole(request, valid, perfLog);

            if (this.config.Prometheus)
            {
                // map category and mode to app values
                string mode = GetMode(perfLog);
                string status = GetPrometheusCode(perfLog.StatusCode);

                this.RequestDuration.WithLabels(status, mode, perfLog.Server, perfLog.Failed.ToString(), this.config.Zone, this.config.Region).Observe(perfLog.Duration);
                this.RequestSummary.WithLabels(status, mode, perfLog.Server, perfLog.Failed.ToString(), this.config.Zone, this.config.Region).Observe(perfLog.Duration);
            }

            return perfLog;
        }

        /// <summary>
        /// Create a PerfLog.
        /// </summary>
        /// <param name="server">server URL.</param>
        /// <param name="request">Request.</param>
        /// <param name="validationResult">validation errors.</param>
        /// <param name="duration">duration.</param>
        /// <param name="contentLength">content length.</param>
        /// <param name="statusCode">status code.</param>
        /// <returns>PerfLog.</returns>
        public PerfLog CreatePerfLog(string server, Request request, ValidationResult validationResult, double duration, long contentLength, int statusCode)
        {
            if (validationResult == null)
            {
                throw new ArgumentNullException(nameof(validationResult));
            }

            // map the parameters
            PerfLog log = new (validationResult.ValidationErrors)
            {
                Server = server,
                Tag = this.config.Tag,
                Path = request?.Path ?? string.Empty,
                StatusCode = statusCode,
                Category = request?.PerfTarget?.Category ?? string.Empty,
                Validated = !validationResult.Failed && validationResult.ValidationErrors.Count == 0,
                Duration = duration,
                ContentLength = contentLength,
                Failed = validationResult.Failed,
                LoadClientId = this.config.LoadClientId,
            };

            // determine the Performance Level based on category
            if (this.targets.ContainsKey(log.Category))
            {
                // lookup the target
                PerfTarget target = this.targets[log.Category];

                if (target != null &&
                    !string.IsNullOrWhiteSpace(target.Category) &&
                    target.Quartiles != null &&
                    target.Quartiles.Count == 3)
                {
                    // set to max
                    log.Quartile = target.Quartiles.Count + 1;

                    for (int i = 0; i < target.Quartiles.Count; i++)
                    {
                        // find the lowest Perf Target achieved
                        if (duration <= target.Quartiles[i])
                        {
                            log.Quartile = i + 1;
                            break;
                        }
                    }
                }
            }

            return log;
        }

        private static string GetMode(PerfLog perfLog)
        {
            string mode = string.IsNullOrEmpty(perfLog.Category) ? string.Empty : perfLog.Category;
            string path = perfLog.Path.ToLower();

            if (path.Contains("healthz"))
            {
                mode = "Healthz";
            }
            else if (mode.StartsWith("Genre") ||
                mode.StartsWith("Rating") ||
                mode.StartsWith("Year") ||
                path.Contains("genres") ||
                mode.ToLowerInvariant().StartsWith("search") ||
                mode.ToLowerInvariant().StartsWith("paged"))
            {
                mode = "Query";
            }

            return mode == "DirectRead" ? "Direct" : mode;
        }

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

        /// <summary>
        /// Display the startup message for RunLoop.
        /// </summary>
        private static void DisplayStartupMessage(Config config)
        {
            Dictionary<string, object> msg = new ()
            {
                { "Date", DateTime.UtcNow },
                { "EventType", "Startup" },
                { "Version", Core.Version.AssemblyVersion },
                { "Host", string.Join(' ', config.Server) },
                { "BaseUrl", config.BaseUrl },
                { "Files", string.Join(' ', config.Files) },
                { "Sleep", config.Sleep },
                { "MaxConcurrent", config.MaxConcurrent },
                { "Duration", config.Duration },
                { "Random", config.Random },
                { "Verbose", config.Verbose },
                { "Tag", config.Tag },
                { "Zone", config.Zone },
                { "Region", config.Region },
            };

            Console.WriteLine(JsonSerializer.Serialize(msg));
        }

        /// <summary>
        /// Open an http client.
        /// </summary>
        /// <param name="index">index of base URL.</param>
        private HttpClient OpenClient(int index)
        {
            if (index < 0 || index >= this.config.Server.Count)
            {
                throw new ArgumentException($"Index out of range: {index}", nameof(index));
            }

            return this.OpenHttpClient(this.config.Server[index]);
        }

        /// <summary>
        /// Opens and configures the shared HttpClient
        ///
        /// Disposed in IDispose.
        /// </summary>
        /// <returns>HttpClient.</returns>
        private HttpClient OpenHttpClient(string host)
        {
            HttpClient client = new (httpSocketHandler)
            {
                Timeout = new TimeSpan(0, 0, this.config.Timeout),
                BaseAddress = new Uri(host),
            };
            client.DefaultRequestHeaders.Add("User-Agent", $"l8r/{Core.Version.ShortVersion}");

            return client;
        }

        /// <summary>
        /// Log the test.
        /// </summary>
        /// <param name="request">Request.</param>
        /// <param name="perfLog">PerfLog.</param>
        private void LogToConsole(Request request, ValidationResult valid, PerfLog perfLog)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (valid == null)
            {
                throw new ArgumentNullException(nameof(valid));
            }

            if (perfLog == null)
            {
                throw new ArgumentNullException(nameof(perfLog));
            }

            // don't log ignore requests
            if (request.PerfTarget?.Category != "Ignore")
            {
                Dictionary<string, object> logDict = new ()
                {
                    { "Date", perfLog.Date },
                    { "Server", perfLog.Server },
                    { "StatusCode", perfLog.StatusCode },
                    { "Verb", request.Verb },
                    { "Path", perfLog.Path },
                    { "Errors", perfLog.ErrorCount },
                    { "Duration", Math.Round(perfLog.Duration, 2) },
                    { "ContentLength", perfLog.ContentLength },
                    { "CVector", perfLog.CorrelationVector },
                    { "CVectorBase", perfLog.CorrelationVectorBase },
                    { "Quartile", perfLog.Quartile },
                    { "Category", perfLog.Category },
                    { "LoadClientId", perfLog.LoadClientId },
                };

                // add zone, region tag
                if (!string.IsNullOrWhiteSpace(this.config.Zone))
                {
                    logDict.Add("Zone", this.config.Zone);
                }

                if (!string.IsNullOrWhiteSpace(this.config.Region))
                {
                    logDict.Add("Region", this.config.Region);
                }

                if (!string.IsNullOrWhiteSpace(this.config.Tag))
                {
                    logDict.Add("Tag", this.config.Tag);
                }

                // log error details
                if (this.config.VerboseErrors && valid.ValidationErrors.Count > 0)
                {
                    string errors = string.Empty;

                    // add up to 5 detailed errors
                    int max = valid.ValidationErrors.Count > 5 ? 5 : valid.ValidationErrors.Count;

                    for (int i = 0; i < max; i++)
                    {
                        errors += valid.ValidationErrors[i].Trim() + "\t";
                    }

                    logDict.Add("ErrorDetails", errors.Trim());
                }

                Console.WriteLine(JsonSerializer.Serialize(logDict, JsonOptions));
            }
        }

        /// <summary>
        /// Called when [test run complete].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="LoadResultEventArgs"/> instance containing the event data.</param>
        private void TestRunComplete(object sender, LoadResultEventArgs args)
        {
            ProcessingEventBus.OnTestRunComplete(sender, args);
        }
    }
}
