// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.API.Data;
using LodeRunner.API.Interfaces;
using LodeRunner.API.Middleware;
using LodeRunner.API.Models;
using LodeRunner.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;

namespace LodeRunner.API.Services
{
    public class LRAPICacheService : ILRAPICacheService
    {
        private readonly LRAPICache cache;

        public LRAPICacheService(ClientStatusService clientStatusService, LoadTestConfigService loadTestConfigService, CancellationTokenSource cancellationTokenSource)
        {
            this.cache = new LRAPICache(clientStatusService, loadTestConfigService, cancellationTokenSource);
        }

        /// <summary>
        /// Gets the client by client status identifier.
        /// </summary>
        /// <param name="clientStatusId">The client status identifier.</param>
        /// <returns>The Client.</returns>
        public Client GetClientByClientStatusId(string clientStatusId)
        {
            return this.cache.GetClientByClientStatusId(clientStatusId);
        }

        /// <summary>
        /// Gets the clients.
        /// </summary>
        /// <returns>The Clients.</returns>
        public IEnumerable<Client> GetClients()
        {
            return this.cache.GetClients();
        }

        /// <summary>
        /// Processes the client status change.
        /// </summary>
        /// <param name="doc">The document.</param>
        public void ProcessClientStatusChange(Document doc)
        {
            this.cache.ProcessClientStatusChange(doc);
        }

        /// <summary>
        /// Handles the cache result.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="results">The results.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>The IActionResult.</returns>
        public IActionResult HandleCacheResult<TEntity>(TEntity results, NgsaLog logger)
        {
            return this.cache.HandleCacheResult(results, logger);
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        /// <returns>The await-able task.</returns>
        public async Task Start()
        {
            // NOTE: Due to lazy construction pattern, this is just a harmless method to Start running this service 'LRAPICacheService'
            // so we trigger the Service Provider to start a new instance  and invoke the constructor to create the LRAPICache.
            // Otherwise the LRAPICache Service will not be created even if swagger web host is up and running, until an API request is submitted.
            await Task.Run(() =>
            {
                // does nothing.
            });
        }
    }
}
