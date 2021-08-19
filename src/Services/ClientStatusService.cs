// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Ngsa.LodeRunner.DataAccessLayer.Interfaces;
using Ngsa.LodeRunner.DataAccessLayer.Model;
using Ngsa.LodeRunner.Interfaces;

namespace Ngsa.LodeRunner.Services
{
    /// <summary>
    ///   Client Status Service
    /// </summary>
    internal class ClientStatusService : IClientStatusService
    {
        private readonly IClientStatusRepository clientStatusRepository;
        public ClientStatusService(IClientStatusRepository clientStatusRepository)
        {
            this.clientStatusRepository = clientStatusRepository;
        }

        public Task PostReady(string message, DateTime? lastUpdated)
        {
            throw new NotImplementedException();
        }

        public async Task PostStarting(string message, DateTime? lastUpdated = null)
        {
            lastUpdated ??= DateTime.Now;

            var entry = new ClientStatus
            {
                EntityType = EntityType.ClientStatus,
                PartitionKey = EntityType.ClientStatus.ToString(),
                StateDuration = -1,
                Status = ClientStatusType.Starting,
                Message = message,
                LastUpdated = lastUpdated ?? DateTime.UtcNow,
                LoadClient = new LoadClient(),
            };

            clientStatusRepository.GenerateId(entry);
            await clientStatusRepository.CreateDocumentAsync(entry).ConfigureAwait(false);
        }

        public Task PostTerminating(string message, DateTime? lastUpdated)
        {
            throw new NotImplementedException();
        }

        public Task PostTesting(string message, DateTime? lastUpdated)
        {
            throw new NotImplementedException();
        }
    }
}
