// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.Core.Interfaces;
using LodeRunner.Core.Models;
using LodeRunner.Core.Models.Validators;
using LodeRunner.Data.Interfaces;

namespace LodeRunner.Services
{
    /// <summary>
    ///   Client Status Service.
    /// </summary>
    public class ClientStatusService : BaseService, IClientStatusService
    {
        private readonly IModelValidator<ClientStatus> validator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientStatusService"/> class.
        /// </summary>
        /// <param name="cosmosDBRepository">The cosmos database repository.</param>
        public ClientStatusService(ICosmosDBRepository cosmosDBRepository)
            : base(cosmosDBRepository)
        {
            this.validator = new ClientStatusValidator();
        }

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
        /// <param name="clientStatus">The ClientStatus entity.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///    The Task[ClientStatus] with updated ClientStatus if CosmosDB post is ready.
        ///    Otherwise, it returns null.
        /// </returns>
        public Task<ClientStatus> PostUpdate(ClientStatus clientStatus, CancellationToken cancellationToken)
        {
            var returnValue = new Task<ClientStatus>(() => null);

            if (clientStatus != null && !cancellationToken.IsCancellationRequested)
            {
                // Update Entity if CosmosDB connection is ready and the object is valid
                if (this.CosmosDBRepository.IsCosmosDBReady().Result && this.validator.ValidateEntity(clientStatus))
                {
                    returnValue = this.CosmosDBRepository.UpsertDocumentAsync(clientStatus, cancellationToken);
                }
                else
                {
                    // TODO: log specific case scenario, even if IsCosmosDBReady() already will do its own logging.

                    // TODO: log validation errors is any  if not  this.validator.IsValid => this.validator.ErrorMessage
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Terminates the service.
        /// </summary>
        /// <param name="clientStatus">The ClientStatus entity.</param>
        public void TerminateService(ClientStatus clientStatus)
        {
            // Update Entity
            clientStatus.LastUpdated = DateTime.UtcNow;
            clientStatus.Message = "Termination requested via Cancellation Token.";
            clientStatus.Status = ClientStatusType.Terminating;

            this.CosmosDBRepository.UpsertDocumentAsync(clientStatus).ConfigureAwait(false);
        }
    }
}
