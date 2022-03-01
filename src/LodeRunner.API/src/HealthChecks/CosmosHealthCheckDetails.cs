// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using LodeRunner.API.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LodeRunner.API
{
    /// <summary>
    /// Cosmos Health Check.
    /// </summary>
    public partial class CosmosHealthCheck : IHealthCheck
    {
        private const int MaxResponseTime = 200;
        private readonly Stopwatch stopwatch = new ();

        private int CosmosTimeoutMs
        {
            get { return this.cosmosConfig.CosmosTimeout * 1000; }
        }

        /// <summary>
        /// Build the response.
        /// </summary>
        /// <param name="uri">string.</param>
        /// <param name="targetDurationMs">double (ms).</param>
        /// <param name="ex">Exception (default = null).</param>
        /// <param name="data">Dictionary(string, object).</param>
        /// <param name="testName">Test Name.</param>
        /// <returns>HealthzCheck.</returns>
        private HealthzCheck BuildHealthzCheck(string uri, double targetDurationMs, Exception ex = null, Dictionary<string, object> data = null, string testName = null)
        {
            this.stopwatch.Stop();

            // create the result
            HealthzCheck result = new ()
            {
                Endpoint = uri,
                Status = HealthStatus.Healthy,
                Duration = this.stopwatch.Elapsed,
                TargetDuration = new System.TimeSpan(0, 0, 0, 0, (int)targetDurationMs),
                ComponentId = testName,
                ComponentType = "datastore",
            };

            // check duration
            if (result.Duration.TotalMilliseconds >= this.CosmosTimeoutMs)
            {
                result.Status = HealthStatus.Unhealthy;
                result.Message = $"{HealthzCheck.TimeoutMessage} of {this.CosmosTimeoutMs} milliseconds";
            }
            else if (result.Duration.TotalMilliseconds > targetDurationMs)
            {
                result.Status = HealthStatus.Degraded;
                result.Message = HealthzCheck.ExceededExpectedDurationMessage;
            }

            // add the exception
            if (ex != null)
            {
                result.Status = HealthStatus.Unhealthy;
                result.Message = ex.Message;
            }

            // add the results to the dictionary
            if (data != null && !string.IsNullOrEmpty(testName))
            {
                data.Add(testName + ":responseTime", result);
            }

            return result;
        }

        /// <summary>
        /// Get Client by ClientStatus Id Healthcheck.
        /// </summary>
        /// <returns>HealthzCheck.</returns>
        private async Task<HealthzCheck> GetClientStatusByClientStatusIdAsync(string clientStatusId, Dictionary<string, object> data = null)
        {
            const string name = "getClientByClientStatusId";
            string path = "/api/clients/" + clientStatusId;

            this.stopwatch.Restart();

            try
            {
                await this.clientStatusService.Get(clientStatusId);

                return this.BuildHealthzCheck(path, MaxResponseTime, null, data, name);
            }
            catch (Exception ex)
            {
                this.BuildHealthzCheck(path, MaxResponseTime, ex, data, name);

                // throw the exception so that HealthCheck logs
                throw;
            }
        }

        /// <summary>
        /// Get Clients Healthcheck.
        /// </summary>
        /// <returns>HealthzCheck.</returns>
        private async Task<HealthzCheck> GetClientStatusesAsync(Dictionary<string, object> data = null)
        {
            const string name = "getClients";

            string path = "/api/clients";

            this.stopwatch.Restart();

            try
            {
                await this.clientStatusService.GetMostRecent(1);

                return this.BuildHealthzCheck(path, MaxResponseTime * 2, null, data, name);
            }
            catch (Exception ex)
            {
                this.BuildHealthzCheck(path, MaxResponseTime * 2, ex, data, name);

                // throw the exception so that HealthCheck logs
                throw;
            }
        }
    }
}
