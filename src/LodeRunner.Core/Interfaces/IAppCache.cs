// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace LodeRunner.Core.Interfaces
{
    /// <summary>
    /// Application Cache Interface.
    /// </summary>
    public interface IAppCache
    {
        /// <summary>
        /// Gets the entries.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns>The Entries as Enumerable.</returns>
        Task<IEnumerable<TEntity>> GetEntries<TEntity>();

        /// <summary>
        /// Gets the entry.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>the cache entry.</returns>
        Task<TEntity> GetEntry<TEntity>(object key);

        /// <summary>
        /// Sets the entry.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="memoryCacheEntryOptions">The memory cache entry options.</param>
        void SetEntry<TEntity>(object key, TEntity value, MemoryCacheEntryOptions memoryCacheEntryOptions);
    }
}
