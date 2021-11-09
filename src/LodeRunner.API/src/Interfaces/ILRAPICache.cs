// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using LodeRunner.API.Data.Dtos;
using LodeRunner.API.Middleware;
using LodeRunner.API.Models;
using LodeRunner.Core.Interfaces;
using LodeRunner.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;

namespace LodeRunner.API.Interfaces
{
    /// <summary>
    /// Data Access Layer for Cache Interface.
    /// </summary>
    public interface ILRAPICache : IAppCache
    {
        /// <summary>
        /// Processes client status item from changefeed.
        /// </summary>
        /// <param name="doc">Changefeed item.</param>
        void ProcessClientStatusChange(Document doc);

        /// <summary>
        /// Gets client by clientStatusId from cache.
        /// </summary>
        /// <param name="clientStatusId">Client status ID.</param>
        /// <returns>Client.</returns>
        Client GetClientByClientStatusId(string clientStatusId);

        /// <summary>
        /// Handles cache results.
        /// </summary>
        /// <typeparam name="TEntity">Entity type.</typeparam>
        /// <param name="results">Results from the cache.</param>
        /// <param name="logger">Logger.</param>
        /// <returns>Action result.</returns>
        IActionResult HandleCacheResult<TEntity>(TEntity results, NgsaLog logger);

        /// <summary>
        /// Gets clients from cache.
        /// </summary>
        /// <returns>Enumerable of clients.</returns>
        IEnumerable<Client> GetClients();

        /// <summary>
        /// Sets the load test configuration.
        /// </summary>
        /// <param name="loadTestConfig">The load test configuration.</param>
        void SetLoadTestConfig(LoadTestConfig loadTestConfig);
    }
}
