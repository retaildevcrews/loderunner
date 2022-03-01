// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.API.Middleware;
using LodeRunner.API.Models;
using LodeRunner.Core.Interfaces;
using LodeRunner.Core.Models;
using LodeRunner.Data.Interfaces;
using LodeRunner.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace LodeRunner.API
{
    /// <summary>
    /// Cosmos Health Check.
    /// </summary>
    public partial class CosmosHealthCheck : IHealthCheck
    {
        /// <summary>
        /// The service identifier.
        /// </summary>
        public const string ServiceId = "LodeRunner.API";

        /// <summary>
        /// The description.
        /// </summary>
        public const string Description = "LodeRunner.API Health Check";

        /// <summary>
        /// The instance.
        /// </summary>
        public const string Instance = "Instance";

        /// <summary>
        /// The version.
        /// </summary>
        public const string Version = "Version";

        /// <summary>
        /// The web site role env variable.
        /// </summary>
        public const string WebSiteRoleEnvVar = "WEBSITE_ROLE_INSTANCE_ID";

        private static JsonSerializerOptions jsonOptions;

        private readonly ILogger logger;
        private readonly IClientStatusService clientStatusService;
        private readonly ICosmosConfig cosmosConfig;

        private ClientStatus clientStatus;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosHealthCheck"/> class.
        /// </summary>
        /// <param name="logger">ILogger.</param>
        /// <param name="clientStatusService">the clientStatusService.</param>
        /// <param name="cosmosConfig">App CosmosConfig Interface.</param>
        public CosmosHealthCheck(ILogger<CosmosHealthCheck> logger, IClientStatusService clientStatusService, ICosmosConfig cosmosConfig)
        {
            // save to member vars
            this.logger = logger;
            this.clientStatusService = clientStatusService;
            this.cosmosConfig = cosmosConfig;

            // setup serialization options
            if (jsonOptions == null)
            {
                // ignore nulls in json
                jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                };

                // serialize enums as strings
                jsonOptions.Converters.Add(new JsonStringEnumConverter());

                // serialize TimeSpan as 00:00:00.1234567
                jsonOptions.Converters.Add(new TimeSpanConverter());
            }
        }

        /// <summary>
        /// Run the health check (IHealthCheck).
        /// </summary>
        /// <param name="context">HealthCheckContext.</param>
        /// <param name="cancellationToken">CancellationToken.</param>
        /// <returns>HealthCheckResult.</returns>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
        {
            // dictionary
            Dictionary<string, object> data = new ();

            // add instance and version
            data.Add(Instance, System.Environment.GetEnvironmentVariable(WebSiteRoleEnvVar) ?? SystemConstants.Unknown);
            data.Add(Version, Middleware.VersionExtension.Version);

            if (cancellationToken.IsCancellationRequested)
            {
                data.Add(SystemConstants.Terminating, SystemConstants.TerminationDescription);
                return new HealthCheckResult(HealthStatus.Unhealthy, Description, data: data);
            }

            try
            {
                HealthStatus status = HealthStatus.Healthy;

                this.clientStatus = CreateClientStatus();

                // create dummy clientStatus record to run health check
                await this.clientStatusService.Post(this.clientStatus, new CancellationTokenSource().Token).ConfigureAwait(false);

                // Run each health check
                await this.GetClientStatusesAsync(data).ConfigureAwait(false);
                await this.GetClientStatusByClientStatusIdAsync(this.clientStatus.Id, data).ConfigureAwait(false);

                // overall health is the worst status
                foreach (object d in data.Values)
                {
                    if (d is HealthzCheck h && h.Status != HealthStatus.Healthy)
                    {
                        status = h.Status;
                    }

                    if (status == HealthStatus.Unhealthy)
                    {
                        break;
                    }
                }

                // return the result
                return new HealthCheckResult(status, Description, data: data);
            }
            catch (CosmosException ce)
            {
                // log and return Unhealthy
                this.logger.LogError($"{ce}\nCosmosException:Healthz:{ce.StatusCode}:{ce.ActivityId}:{ce.Message}");

                data.Add("CosmosException", ce.Message);

                return new HealthCheckResult(HealthStatus.Unhealthy, Description, ce, data);
            }
            catch (Exception ex)
            {
                // log and return unhealthy
                this.logger.LogError($"{ex}\nException:Healthz:{ex.Message}");
                data.Add("Exception", ex.Message);

                return new HealthCheckResult(HealthStatus.Unhealthy, Description, ex, data);
            }
            finally
            {
                try
                {
                    // delete clientStatus record
                    await this.clientStatusService.Delete(this.clientStatus.Id);
                }
                catch (Exception ex)
                {
                    // ignore the exception here, since ttl feature should delete clientStatus record
                    this.logger.LogError($"{ex}\nException:Delete Healthz test clientStatus:{ex.Message}");
                }
            }
        }

        /// <summary>
        /// Create ClientStatus object.
        /// </summary>
        /// <returns>ClientStatus.</returns>
        private static ClientStatus CreateClientStatus()
        {
            LoadClient loadClient = new ()
            {
                Version = "1.0.0",
                Region = $"CosmosHealthCheck-{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}",
                StartupArgs = $"--secrets-volume secrets",
                StartTime = DateTime.UtcNow,
                Name = $"CosmosHealthCheck-{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}",
            };

            return new ClientStatus
            {
                Status = ClientStatusType.Starting,
                LoadClient = loadClient,
            };
        }
    }
}
