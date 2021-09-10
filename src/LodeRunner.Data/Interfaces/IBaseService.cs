// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace LodeRunner.Data.Interfaces
{
    /// <summary>
    /// ClientStatusService Interface.
    /// </summary>
    public interface IBaseService
    {
        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="id">The identifier.</param>
        /// <returns>The corresponding Entity.</returns>
        Task<TEntity> Get<TEntity>(string id);

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns>all items for a given type.</returns>
        Task<IEnumerable<TEntity>> GetAll<TEntity>();

        /// <summary>
        /// Gets the most recent asynchronous.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="limit">The limit.</param>
        /// <returns>all items for a given type.</returns>
        Task<IEnumerable<TEntity>> GetMostRecentAsync<TEntity>(int limit = 1);

        /// <summary>
        /// Gets the count asynchronous.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns>Item count that match the Entity type.</returns>
        Task<int> GetCountAsync<TEntity>();
    }
}
