// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using LodeRunner.API.Middleware;
using LodeRunner.API.Models;
using LodeRunner.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;

namespace LodeRunner.API.Interfaces
{
    /// <summary>
    /// The LRAPI Cache Service
    /// </summary>
    /// <seealso cref="LodeRunner.Core.Interfaces.ICacheService" />
    public interface ILRAPICacheService : ICacheService
    {
        IEnumerable<Client> GetClients();
        Client GetClientByClientStatusId(string clientStatusId);
        void ProcessClientStatusChange(Document doc);

        IActionResult HandleCacheResult<TEntity>(TEntity results, NgsaLog logger);

        /// <summary>
        /// Starts this instance.
        /// </summary>
        /// <returns>The await-able task.</returns>
        Task Start();
    }
}
