// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using LodeRunner.API.Middleware;
using LodeRunner.API.Models;
using LodeRunner.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;

namespace LodeRunner.API.Interfaces
{
    /// <summary>
    /// Data Access Layer for Cache Interface
    /// </summary>
    public interface ILRAPCache : IAppCache
    {
        void ProcessClientStatusChange(Document doc);

        Client GetClientByClientStatusId(string clientStatusId);

        IActionResult HandleCacheResult<TFlattenEntity>(TFlattenEntity results, NgsaLog logger);

        IEnumerable<Client> GetClients();
    }
}
