// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
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

        public LRAPICacheService(ClientStatusService clientStatusService, LoadTestConfigService loadTestConfigService)
        {
            this.cache = new LRAPICache(clientStatusService, loadTestConfigService);
        }

        public Client GetClientByClientStatusId(string clientStatusId)
        {
            return this.cache.GetClientByClientStatusId(clientStatusId);
        }

        public IEnumerable<Client> GetClients()
        {
            return this.cache.GetClients();
        }

        public void ProcessClientStatusChange(Document doc)
        {
            this.cache.ProcessClientStatusChange(doc);
        }

        public IActionResult HandleCacheResult<TFlattenEntity>(TFlattenEntity results, NgsaLog logger)
        {
            return this.cache.HandleCacheResult(results, logger);
        }
    }
}
