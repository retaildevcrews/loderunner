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
    public class ClientStatusService : BaseService<ClientStatus>, IClientStatusService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientStatusService"/> class.
        /// </summary>
        /// <param name="cosmosDBRepository">The cosmos database repository.</param>
        public ClientStatusService(ICosmosDBRepository cosmosDBRepository)
            : base(cosmosDBRepository)
        {
            this.Validator = new ClientStatusValidator();
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
        public async Task<ClientStatus> PostUpdate(ClientStatus clientStatus, CancellationToken cancellationToken)
        {
            return await this.Save(clientStatus, cancellationToken);
        }

        /// <summary>
        /// Terminates the service.
        /// </summary>
        /// <param name="clientStatus">The ClientStatus entity.</param>
        public void TerminateService(ClientStatus clientStatus)
        {
            // Update Entity
            // clientStatus.LastUpdated = DateTime.UtcNow;
            clientStatus.Message = "Termination requested via Cancellation Token.";
            clientStatus.Status = ClientStatusType.Terminating;

            this.CosmosDBRepository.UpsertDocumentAsync(clientStatus).ConfigureAwait(false);
        }
    }
}
