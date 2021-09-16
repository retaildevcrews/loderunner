// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.Core.Models;

namespace LodeRunner.Data.Interfaces
{
    /// <summary>
    /// ClientStatusService Interface.
    /// </summary>
    public interface IClientStatusService
    {
        /// <summary>
        /// Posts the update.
        /// </summary>
        /// <param name="clientStatus">The ClientStatus entity.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The Updated clientStatus entity.</returns>
        Task<ClientStatus> PostUpdate(ClientStatus clientStatus, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The corresponding Entity.</returns>
        Task<ClientStatus> Get(string id);

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns>all items for a given type.</returns>
        Task<IEnumerable<ClientStatus>> GetAll();

        /// <summary>
        /// Gets the most recent asynchronous.
        /// </summary>
        /// <param name="limit">The limit.</param>
        /// <returns>all the number of items for a given type.</returns>
        Task<IEnumerable<ClientStatus>> GetMostRecent(int limit = 1);

        /// <summary>
        /// Gets the count asynchronous.
        /// </summary>
        /// <returns>Items Count EntityType equals ClientStatus.</returns>
        Task<int> GetCount();

        /// <summary>
        /// Terminates the service.
        /// </summary>
        /// <param name="clientStatus">The ClientStatus entity.</param>
        void TerminateService(ClientStatus clientStatus);
    }
}
