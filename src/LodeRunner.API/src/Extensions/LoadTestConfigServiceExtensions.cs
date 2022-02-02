// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LodeRunner.API.Middleware;
using LodeRunner.Core.Models;
using LodeRunner.Data.Interfaces;
using LodeRunner.Services;
using Microsoft.Azure.Cosmos;

namespace LodeRunner.API.Extensions
{
    /// <summary>
    /// LoadTestConfigService Extension methods.
    /// </summary>
    public static class LoadTestConfigServiceExtensions
    {
        /// <summary>
        /// Gets all load test configurations.
        /// </summary>
        /// <param name="loadTestConfigService">The load test config service.</param>
        /// <returns>The Task</returns>
        public static async Task<IEnumerable<LoadTestConfig>> GetLoadTestConfigs(this ILoadTestConfigService loadTestConfigService)
        {
            // List of all clients
            return (List<LoadTestConfig>)await loadTestConfigService.GetAll();
        }

        /// <summary>
        /// Gets the LoadTestConfig by identifier.
        /// </summary>
        /// <param name="loadTestConfigService">The test run service.</param>
        /// <param name="loadTestConfigId">The test run identifier.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>The Task.</returns>
        public static async Task<LoadTestConfig> GetLoadTestConfigById(this LoadTestConfigService loadTestConfigService, string loadTestConfigId, NgsaLog logger)
        {
            LoadTestConfig result = null;
            try
            {
                // Get Test Run from Cosmos

                LoadTestConfig loadTestConfig = await loadTestConfigService.Get(loadTestConfigId);

                if (loadTestConfig != null)
                {
                    result = loadTestConfig;
                }
            }
            catch (CosmosException ce)
            {
                // log and return Cosmos status code
                if (ce.StatusCode == HttpStatusCode.NotFound)
                {
                    await logger.LogWarning(nameof(GetLoadTestConfigById), logger.NotFoundError, new LogEventId((int)ce.StatusCode, string.Empty));
                }
                else
                {
                    throw new Exception($"{nameof(GetLoadTestConfigById)}: {ce.Message}", ce);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(GetLoadTestConfigById)}: {ex.Message}", ex);
            }

            return result;
        }
    }
}
