// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
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
        private readonly ILoadClientRepository loadClientRepository;
        private readonly IModelValidator<ClientStatus> validator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientStatusService"/> class.
        /// </summary>
        /// <param name="clientStatusRepository">The client status repository.</param>
        /// <param name="loadClientRepository">The load client repository.</param>
        public ClientStatusService(IClientStatusRepository clientStatusRepository, ILoadClientRepository loadClientRepository)
        {
            this.clientStatusRepository = clientStatusRepository;
            this.loadClientRepository = loadClientRepository;

            this.validator = new ClientStatusValidator();
        }

        /// <summary>
        /// Posts the specified status.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="clientStatus">The ClientStatus entity.</param>
        /// <param name="lastUpdated">The last updated.</param>
        /// <param name="status">The status.</param>
        /// <returns>
        /// The Task.
        /// </returns>
        public async Task<ClientStatus> Post(string message, ClientStatus clientStatus, DateTime lastUpdated, ClientStatusType status)
        {
            return status switch
            {
                ClientStatusType.Ready => await this.PostUpdate(message, clientStatus, lastUpdated, status),
                ClientStatusType.Starting => await this.PostStarting(message, lastUpdated),
                ClientStatusType.Testing => await this.PostUpdate(message, clientStatus, lastUpdated, status),
                ClientStatusType.Terminating => await this.PostUpdate(message, clientStatus, lastUpdated, status),

                // TODO: how to invalid ClientType
                _ => throw new ApplicationException($"ClientStatus Service Post failed - Status type {status} is invalid."),
            };
        }

        /// <summary>
        /// Posts the starting.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="lastUpdated">The last updated.</param>
        /// <returns>
        /// The Task.
        /// </returns>
        public async Task<ClientStatus> PostStarting(string message, DateTime lastUpdated)
        {
            // TODO: need to set the correct data, this is just and example
            // Create a new ClientStatus Entry
            var clientStatusEntry = new ClientStatus
            {
                EntityType = EntityType.ClientStatus,
                PartitionKey = EntityType.ClientStatus.ToString(),
                StatusDuration = 1,
                Status = ClientStatusType.Starting,
                Message = message,
                LastUpdated = lastUpdated,
                LoadClient = new LoadClient
                {
                    EntityType = EntityType.LoadClient,
                    Version = "0.3.0 - 717 - 1030",
                    Name = "Central - az - central - us - 2",
                    Region = "Central",
                    Zone = "az-central-us",
                    Prometheus = false,
                    StartupArgs = "--delay - start - 1--secrets - volume secrets",
                    StartTime = lastUpdated,
                },
            };

            // Generate Entry Id
            this.clientStatusRepository.GenerateId(clientStatusEntry);

            this.loadClientRepository.GenerateId(clientStatusEntry.LoadClient);

            // Validate Entry
            var errors = this.validator.Validate(clientStatusEntry).Errors.ToList();
            if (errors.Count > 0)
            {
                var errorsList = errors.Select(x => $"{x.PropertyName} - {x.ErrorMessage}").ToList<string>();
                var errorMsg = string.Join('\n', errorsList);

                // TODO: how to handle validation errors
                throw new ApplicationException($"PostStarting validation failed - {message}\n{errorMsg}\n\n");
            }
            else
            {
                var createStatusTask = Task.Run(() => this.clientStatusRepository.CreateDocumentAsync(clientStatusEntry).Result);

                // NOTE: We need to make sure the item is created before to move on since it is Cached at the LodeService and utilized to update Status later on.
                createStatusTask.Wait();

                return await createStatusTask;
            }
        }

        /// <summary>
        /// Posts the ready.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="clientStatus">The ClientStatus entity.</param>
        /// <param name="lastUpdated">The last updated.</param>
        /// <param name="status">Client Status Type.</param>
        /// <returns>
        /// The Task.
        /// </returns>
        public async Task<ClientStatus> PostUpdate(string message, ClientStatus clientStatus, DateTime lastUpdated, ClientStatusType status)
        {
            if (clientStatus == null)
            {
                // TODO: What to if it is null
                throw new ApplicationException($"PostReady failed - clientStatus cannot be null.");
            }

            // Update Entity
            clientStatus.LastUpdated = lastUpdated;
            clientStatus.Message = message;
            clientStatus.Status = status;

            // Validate Entity
            var errors = this.validator.Validate(clientStatus).Errors.ToList();
            if (errors.Count > 0)
            {
                var errorsList = errors.Select(x => $"{x.PropertyName} - {x.ErrorMessage}").ToList<string>();
                var errorMsg = string.Join('\n', errorsList);

                // TODO: how to handle validation errors
                throw new ApplicationException($"PostReady validation failed - {message}\n{errorMsg}\n\n");
            }
            else
            {
                if (status == ClientStatusType.Terminating)
                {
                    var terminatingStatusTask = Task.Run(() => this.clientStatusRepository.UpsertDocumentAsync(clientStatus).Result);

                    // NOTE: We need to make sure the update is posted back to Cosmos before to terminate the application.
                    terminatingStatusTask.Wait();

                    return await terminatingStatusTask;
                }
                else
                {
                    return await this.clientStatusRepository.UpsertDocumentAsync(clientStatus).ConfigureAwait(false);
                }
            }
        }
    }
}
