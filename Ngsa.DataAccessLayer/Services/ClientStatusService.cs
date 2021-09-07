// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        private readonly ClientStatus clientStatus;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientStatusService"/> class.
        /// </summary>
        /// <param name="cosmosDBRepository">The cosmos database repository.</param>
        /// <param name="clientStatus">The ClientStatus entity.</param>
        /// <param name="cancellationTokenSource">The cancellation Token Source.</param>
        public ClientStatusService(ICosmosDBRepository cosmosDBRepository, ClientStatus clientStatus, CancellationTokenSource cancellationTokenSource)
            : base(cosmosDBRepository)
        {
            this.validator = new ClientStatusValidator();
            this.clientStatus = clientStatus;
            cancellationTokenSource.Token.Register(this.TerminateService);
        }

        /// <summary>
        /// Gets the client status.
        /// </summary>
        /// <value>
        /// The client status.
        /// </value>
        public ClientStatus ClientStatus => this.clientStatus;

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// The corresponding Entity.
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
        /// Posts the update.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="lastUpdated">The last updated.</param>
        /// <param name="status">The status.</param>
        /// <param name="cancellationToken">the cancellation token.</param>
        /// <returns>
        /// The Task.
        /// </returns>
        public Task<ClientStatus> PostUpdate(string message, DateTime lastUpdated, ClientStatusType status, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return new Task<ClientStatus>(() => this.clientStatus);
            }

            if (this.clientStatus == null)
            {
                // TODO: What to do if it is null
                throw new ApplicationException($"PostReady failed - clientStatus cannot be null.");
            }

            lock (this.clientStatus)
            {
                // Update Entity
                this.clientStatus.LastUpdated = lastUpdated;
                this.clientStatus.Message = message;
                this.clientStatus.Status = status;

                // Validate Entity
                if (!this.ValidateEntity(message))
                {
                    // we return a task with the current ClientStatus.
                    return new Task<ClientStatus>(() => this.clientStatus);
                }

                // Should I check for IsCosmosDBReady
                if (this.CosmosDBRepository.IsCosmosDBReady().Result)
                {
                    return this.CosmosDBRepository.UpsertDocumentAsync(this.clientStatus, cancellationToken);
                }
                else
                {
                    // TODO: log specific case scenario, even if IsCosmosDBReady() already will do its own logging.

                    // we return a task with the current ClientStatus.
                    return new Task<ClientStatus>(() => this.clientStatus);
                }
            }
        }

        /// <summary>
        /// Terminates the service.
        /// </summary>
        private void TerminateService()
        {
            // Update Entity
            this.clientStatus.LastUpdated = DateTime.UtcNow;
            this.clientStatus.Message = "Termination requested via Cancellation Token.";
            this.clientStatus.Status = ClientStatusType.Terminating;

            this.CosmosDBRepository.UpsertDocumentAsync(this.clientStatus).ConfigureAwait(false);
        }

        /// <summary>
        /// Validates the entity.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>True if entity passed IModelValidator validation, otherwise false.</returns>
        private bool ValidateEntity(string message)
        {
            var errors = this.validator.Validate(this.clientStatus).Errors.ToList();
            if (errors.Count > 0)
            {
                var errorsList = errors.Select(x => $"{x.PropertyName} - {x.ErrorMessage}").ToList<string>();
                var errorMsg = string.Join('\n', errorsList);

                // TODO: how to handle validation errors
                throw new ApplicationException($"PostReady validation failed - {message}\n{errorMsg}\n\n");
            }
            else
            {
                return true;
            }
        }
    }
}
