// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LodeRunner.API.Data;
using LodeRunner.API.Interfaces;
using LodeRunner.API.Models;
using LodeRunner.Services;
using Microsoft.Azure.Documents;

namespace LodeRunner.API.Services
{
    public class LRAPICacheService : ILRAPICacheService
    {
        private readonly Cache cache;

        public LRAPICacheService(ClientStatusService clientStatusService, LoadTestConfigService loadTestConfigService)
        {
            // TODO: refactor 'Cache' to be Generic.

            this.cache = new Cache(clientStatusService, loadTestConfigService);
        }

        public Client GetClientByClientStatusId(string clientStatusId)
        {
            return this.cache.GetClientByClientStatusId(clientStatusId);
        }

        public IEnumerable<Client> GetClients()
        {
            return this.GetClients();
        }

        public void ProcessClientStatusChange(Document doc)
        {
            this.cache.ProcessClientStatusChange(doc);
        }

        // TODO: Add other method to get or process other object types, at this time the Service is just a Wrapper and Cache needs to be refactored to be type Independent. "Generic"
    }
}
