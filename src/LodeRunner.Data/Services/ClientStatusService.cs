// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
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
