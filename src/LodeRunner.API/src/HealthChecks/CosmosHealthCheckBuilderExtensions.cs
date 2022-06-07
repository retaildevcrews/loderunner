// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using LodeRunner.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LodeRunner.API
{
    /// <summary>
    /// Cosmos health check builder.
    /// </summary>
    public static class CosmosHealthCheckBuilderExtensions
    {
        /// <summary>
        /// Adds cosmos health check.
        /// </summary>
        /// <param name="builder">Health checks builder.</param>
        /// <param name="name">Name of cosmos health check.</param>
        /// <param name="failureStatus">Health check, failure status.</param>
        /// <param name="tags">Tags.</param>
        /// <returns>Builder.</returns>
        public static IHealthChecksBuilder AddCosmosHealthCheck(
            this IHealthChecksBuilder builder,
            string name,
            HealthStatus? failureStatus = null,
            IEnumerable<string> tags = null)
        {
            // Register a check of type Cosmos
            builder.AddCheck<CosmosHealthCheck>(name, failureStatus ?? HealthStatus.Degraded, tags);

            return builder;
        }

        /// <summary>
        /// Gets the content result based on healthCheckResult.Status.
        /// </summary>
        /// <param name="healthCheckResult">The health check result.</param>
        /// <returns>The associated ContentResult.</returns>
        public static ContentResult GetContentResult(this HealthCheckResult healthCheckResult)
        {
            object innerMessage = string.Empty;
            if (healthCheckResult.Status == HealthStatus.Unhealthy)
            {
                _ = healthCheckResult.Data.TryGetValue(SystemConstants.Terminating, out innerMessage);

                if (innerMessage != null)
                {
                    innerMessage = $": {SystemConstants.Terminating} - {innerMessage}";
                }
            }

            ContentResult result = new()
            {
                Content = $"{IetfCheck.ToIetfStatus(healthCheckResult.Status)}{innerMessage}",
                StatusCode = healthCheckResult.Status == HealthStatus.Healthy ? (int)System.Net.HttpStatusCode.OK : (int)System.Net.HttpStatusCode.ServiceUnavailable,
            };

            return result;
        }
    }
}
