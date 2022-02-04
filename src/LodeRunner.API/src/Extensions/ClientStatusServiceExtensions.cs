// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using LodeRunner.API.Models;
using LodeRunner.Data.Interfaces;

namespace LodeRunner.API.Extensions
{
    public static class ClientStatusServiceExtensions
    {
        /// <summary>
        /// Gets the client by identifier.
        /// </summary>
        /// <param name="clientStatusService">The client status service.</param>
        /// <param name="clientStatusId">The client status identifier.</param>
        /// <returns>The Task.</returns>
        public static async Task<Client> GetClientByClientStatusId(this IClientStatusService clientStatusService, string clientStatusId)
        {
            // Get Client from Cosmos
            var clientStatus = await clientStatusService.Get(clientStatusId);
            return new Client(clientStatus);
        }
    }
}
