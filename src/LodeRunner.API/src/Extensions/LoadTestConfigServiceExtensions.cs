// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net;
using System.Threading.Tasks;
using LodeRunner.Core.Models;
using LodeRunner.Data.Interfaces;
using LodeRunner.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace LodeRunner.API.Extensions
{
    /// <summary>
    /// LoadTestConfigService Extension methods.
    /// </summary>
    public static class LoadTestConfigServiceExtensions
    {
        /// <summary>
        /// Gets the LoadTestConfig by identifier.
        /// </summary>
        /// <param name="loadTestConfigService">The test run service.</param>
        /// <param name="loadTestConfigId">The test run identifier.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>The Task.</returns>
        public static async Task<LoadTestConfig> GetLoadTestConfigById(this LoadTestConfigService loadTestConfigService, string loadTestConfigId, ILogger logger)
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
                    logger.LogWarning(new EventId((int)ce.StatusCode, nameof(GetLoadTestConfigById)), $"Load Test Configs {SystemConstants.NotFoundError}");
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
