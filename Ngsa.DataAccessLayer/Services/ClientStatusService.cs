// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ngsa.DataAccessLayer.Model.Validators;
using Ngsa.LodeRunner.DataAccessLayer.Interfaces;
using Ngsa.LodeRunner.DataAccessLayer.Model;

namespace Ngsa.LodeRunner.Services
{
    /// <summary>
    ///   Client Status Service.
    /// </summary>
    public class ClientStatusService : IClientStatusService
    {
        private readonly IClientStatusRepository clientStatusRepository;
        private readonly IModelValidator<ClientStatus> validator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientStatusService"/> class.
        /// </summary>
        /// <param name="clientStatusRepository">The client status repository.</param>
        public ClientStatusService(IClientStatusRepository clientStatusRepository)
        {
            this.clientStatusRepository = clientStatusRepository;

            this.validator = new ClientStatusValidator();
        }

        /// <summary>
        /// Posts the specified status.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <param name="message">The message.</param>
        /// <param name="lastUpdated">The last updated.</param>
        /// <returns>The Task.</returns>
        public async Task Post(ClientStatusType status, string message, DateTime? lastUpdated)
        {
            switch (status)
            {
                case ClientStatusType.Ready: await this.PostReady(message, lastUpdated); break;
                case ClientStatusType.Starting: await this.PostStarting(message, lastUpdated); break;
                case ClientStatusType.Terminating: await this.PostTerminating(message, lastUpdated); break;
                case ClientStatusType.Testing: await this.PostTesting(message, lastUpdated); break;
                default: throw new ApplicationException($"ClientStatus Service Post failed - Status type {status} is invalid.");
            }
        }

        /// <summary>
        /// Posts the ready.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="lastUpdated">The last updated.</param>
        /// <returns>
        /// The Task.
        /// </returns>
        public Task PostReady(string message, DateTime? lastUpdated)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Posts the starting.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="lastUpdated">The last updated.</param>
        /// <returns>Created Document task.</returns>
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

            this.clientStatusRepository.GenerateId(entry);

            var errors = this.validator.Validate(entry).Errors.ToList();
            if (errors.Count > 0)
            {
                var errorMsg = string.Join('\n', errors);
                throw new ApplicationException($"PostStarting validation failed - {message}\n{errorMsg}\n\n");
            }
            else
            {
                await this.clientStatusRepository.CreateDocumentAsync(entry).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Posts the terminating.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="lastUpdated">The last updated.</param>
        /// <returns>
        /// The Task.
        /// </returns>
        public Task PostTerminating(string message, DateTime? lastUpdated)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Posts the testing.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="lastUpdated">The last updated.</param>
        /// <returns>
        /// The Task.
        /// </returns>
        public Task PostTesting(string message, DateTime? lastUpdated)
        {
            throw new NotImplementedException();
        }
    }
}
