// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ngsa.LodeRunner.DataAccessLayer.Model;

namespace Ngsa.LodeRunner.DataAccessLayer.Interfaces
{
    /// <summary>
    /// ClientStatusService Interface.
    /// </summary>
    public interface IClientStatusService
    {
        /// <summary>
        /// Posts the update.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="lastUpdated">The last updated.</param>
        /// <param name="status">The status.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The Updated clientStatus entity.</returns>
        Task<ClientStatus> PostUpdate(string message, DateTime lastUpdated, ClientStatusType status, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The corresponding Enity.</returns>
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
    }
}
