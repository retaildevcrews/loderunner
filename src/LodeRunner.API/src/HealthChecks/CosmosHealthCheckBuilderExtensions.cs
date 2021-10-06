// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
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
    }
}
