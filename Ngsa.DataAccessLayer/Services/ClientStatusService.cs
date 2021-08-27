// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ngsa.DataAccessLayer.Interfaces;
using Ngsa.DataAccessLayer.Model.Validators;
using Ngsa.LodeRunner.DataAccessLayer.Interfaces;
using Ngsa.LodeRunner.DataAccessLayer.Model;

namespace Ngsa.LodeRunner.Services
{
    /// <summary>
    ///   Client Status Service.
    /// </summary>
    public class ClientStatusService : BaseService, IClientStatusService
    {
        private readonly IModelValidator<ClientStatus> validator;
        private readonly IConfig config;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientStatusService"/> class.
        /// </summary>
        /// <param name="cosmosDBRepository">The cosmos database repository.</param>
        /// <param name="config">The loderunner configuration.</param>
        public ClientStatusService(ICosmosDBRepository cosmosDBRepository, IConfig config)
            : base(cosmosDBRepository)
        {
            this.config = config;
            this.validator = new ClientStatusValidator();
        }

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// The corresponding Enity.
        /// </returns>
        public async Task<ClientStatus> Get(string id)
        {
            return await this.Get<ClientStatus>(id);
        }

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns>
        /// all items for a given type.
        /// </returns>
        public async Task<IEnumerable<ClientStatus>> GetAll()
        {
            return await this.GetAll<ClientStatus>();
        }

        /// <summary>
        /// Gets the count asynchronous.
        /// </summary>
        /// <returns>
        /// Items Count EntityType equals ClientStatus.
        /// </returns>
        public async Task<int> GetCount()
        {
            return await this.GetCountAsync<ClientStatus>();
        }

        /// <summary>
        /// Gets the most recent asynchronous.
        /// </summary>
        /// <param name="limit">The limit.</param>
        /// <returns>
        /// all the number of items for a given type.
        /// </returns>
        public async Task<IEnumerable<ClientStatus>> GetMostRecent(int limit = 1)
        {
            return await this.GetMostRecentAsync<ClientStatus>(limit);
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
            // TODO: need to set the correct data, this is just an example
            // Create a new ClientStatus Entry
            var clientStatusEntry = new ClientStatus
            {
                StatusDuration = 1,
                Status = ClientStatusType.Starting,
                Message = message,
                LastUpdated = lastUpdated,
                LoadClient = LoadClient.GetNew(this.config, lastUpdated),
            };

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
                var createStatusTask = Task.Run(() => this.CosmosDBRepository.CreateDocumentAsync(clientStatusEntry).Result);

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
                    var terminatingStatusTask = Task.Run(() => this.CosmosDBRepository.UpsertDocumentAsync(clientStatus).Result);

                    // NOTE: We need to make sure the update is posted back to Cosmos before to terminate the application.
                    terminatingStatusTask.Wait();

                    return await terminatingStatusTask;
                }
                else
                {
                    return await this.CosmosDBRepository.UpsertDocumentAsync(clientStatus).ConfigureAwait(false);
                }
            }
        }
    }
}
