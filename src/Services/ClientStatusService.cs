// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ngsa.DataAccessLayer.Model.Validators;
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
        private readonly IModelValidator<ClientStatus> validator;

        public ClientStatusService(IClientStatusRepository clientStatusRepository)
        {
            this.clientStatusRepository = clientStatusRepository;

            this.validator = new ClientStatusValidator();
        }

        public Task PostReady(string message, DateTime? lastUpdated)
        {
            throw new NotImplementedException();
        }

        public async Task PostStarting(string message, DateTime? lastUpdated = null)
        {
            lastUpdated ??= DateTime.UtcNow;

            var entry = new ClientStatus
            {
                EntityType = EntityType.ClientStatus,
                PartitionKey = EntityType.ClientStatus.ToString(),
                StatusDuration = -1,
                Status = ClientStatusType.Starting,
                Message = message,
                LastUpdated = lastUpdated ?? DateTime.UtcNow,
                LoadClient = new LoadClient(),
            };

            clientStatusRepository.GenerateId(entry);

            var errors = this.validator.Validate(entry).Errors.ToList();
            if (errors.Count > 0)
            {
                var errorMsg = string.Join('\n', errors);
                throw new ApplicationException($"PostStarting validation failed - {message}\n{errorMsg}\n\n");
            }
            else
            {
                await clientStatusRepository.CreateDocumentAsync(entry).ConfigureAwait(false);
            }
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
