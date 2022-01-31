// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using LodeRunner.API.Models;
using LodeRunner.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace LodeRunner.API.Extensions
{
    public static class ClientStatusServiceExtensions
    {
        /// <summary>
        /// Gets all clients.
        /// </summary>
        /// <param name="clientStatusService">The client status service.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>The Task</returns>
        public static async Task<List<Client>> GetClients(this ClientStatusService clientStatusService, ILogger logger)
        {
            List<Client> result = new ();
            try
            {
                // client statuses
                var clientStatusList = await clientStatusService.GetAll();
                foreach (var item in clientStatusList)
                {
                    result.Add(new Client(item));
                }
            }
            catch (CosmosException ce)
            {
                // log and return Cosmos status code
                if (ce.StatusCode == HttpStatusCode.NotFound)
                {
                    logger.LogWarning(new EventId((int)ce.StatusCode, nameof(GetClients)), $"Clients {SystemConstants.NotFoundError}");
                }
                else
                {
                    throw new Exception($"{nameof(GetClients)}: {ce.Message}", ce);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(GetClients)}: {ex.Message}", ex);
            }

            return result;
        }

        /// <summary>
        /// Gets the client by identifier.
        /// </summary>
        /// <param name="clientStatusService">The client status service.</param>
        /// <param name="clientStatusId">The client status identifier.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>The Task.</returns>
        public static async Task<Client> GetClientByClientStatusId(this ClientStatusService clientStatusService, string clientStatusId, ILogger logger)
        {
            Client result = null;
            try
            {
                // Get Client Status from Cosmos

                var clientStatus = await clientStatusService.Get(clientStatusId);

                if (clientStatus != null)
                {
                    result = new Client(clientStatus);
                }
            }
            catch (CosmosException ce)
            {
                // log and return Cosmos status code
                if (ce.StatusCode == HttpStatusCode.NotFound)
                {
                    logger.LogWarning(new EventId((int)ce.StatusCode, nameof(GetClientByClientStatusId)), $"Clients {SystemConstants.NotFoundError}");
                }
                else
                {
                    throw new Exception($"{nameof(GetClientByClientStatusId)}: {ce.Message}", ce);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(GetClientByClientStatusId)}: {ex.Message}", ex);
            }

            return result;
        }
    }
}
