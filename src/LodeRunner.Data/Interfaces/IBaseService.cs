﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LodeRunner.Data.Interfaces
{
    /// <summary>
    /// BaseService Interface.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IBaseService<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The corresponding Entity.</returns>
        Task<TEntity> Get(string id);

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns>all items for a given type.</returns>
        Task<IEnumerable<TEntity>> GetAll();

        /// <summary>
        /// Gets the most recent asynchronous.
        /// </summary>
        /// <param name="limit">The limit.</param>
        /// <returns>all items for a given type.</returns>
        Task<IEnumerable<TEntity>> GetMostRecent(int limit = 1);

        /// <summary>
        /// Gets the count asynchronous.
        /// </summary>
        /// <returns>Item count that match the Entity type.</returns>
        Task<int> GetCount();

        /// <summary>
        /// Posts the specified entity.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The Created or Updated Entity.</returns>
        Task<TEntity> Save(TEntity model, CancellationToken cancellationToken);
    }
}
