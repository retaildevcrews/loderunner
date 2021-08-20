// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ngsa.LodeRunner.DataAccessLayer.Interfaces;
using Ngsa.LodeRunner.Interfaces;
using Ngsa.LodeRunner.Services;

namespace Ngsa.LodeRunner.DataAccessLayer
{
    public class DalManager : IDalManager
    {
        private IClientStatusService clientStatusService;

        public DalManager(Config config)
        {
            CreateServices(config);
        }

        public IClientStatusService ClientStatusService { get => clientStatusService; private set => clientStatusService = value; }

        private async void CreateServices(Config config)
        {
            var cosmosDBSettings = new ClientStatusRepositorySettings()
            {
                CollectionName = config.Secrets.CosmosCollection,
                Retries = config.Retries,
                Timeout = config.CosmosTimeout,
                Uri = config.Secrets.CosmosServer,
                Key = config.Secrets.CosmosKey,
                DatabaseName = config.Secrets.CosmosDatabase,
            };

            cosmosDBSettings.Validate();

            var clientStatusRespository = new ClientStatusRepository(cosmosDBSettings);
            clientStatusService = new ClientStatusService(clientStatusRespository);

            try
            {
                await ClientStatusService.PostStarting("Starting LodeRunner").ConfigureAwait(false);
            }
            catch (Exception)
            {
                // TODO: how do we handle Key rotation
                // https://docs.microsoft.com/en-us/azure/cosmos-db/troubleshoot-unauthorized
            }

            //TODO: create other services
        }
    }
}
