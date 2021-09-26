﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;

namespace LodeRunner.Data.Interfaces
{
    /// <summary>
    /// Data Access Layer for Cache Interface.
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// Gets the entries.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="keyPrefix">The key prefix.</param>
        /// <returns>The Entries as Enumerable.</returns>
        IEnumerable<TEntity> GetEntries<TEntity>(string keyPrefix);

        /// <summary>
        /// Sets the entries.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TFlattenEntity">The type of the flatten entity.</typeparam>
        /// <param name="items">The items.</param>
        /// <param name="keyPrefix">The key prefix.</param>
        /// <param name="entityIdFieldName">Name of the entity Id Field.</param>
        void SetEntries<TEntity, TFlattenEntity>(List<TEntity> items, string keyPrefix, string entityIdFieldName = "Id");

        /// <summary>
        /// Gets the entry by key.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="keyPrefix">The key prefix.</param>
        /// <param name="key">The key.</param>
        /// <returns>Cache Entry As TEntity type.</returns>
        TEntity GetEntryByKey<TEntity>(string keyPrefix, string key);

        /// <summary>
        /// Gets the entry.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>the cache entry.</returns>
        object GetEntry<TEntity>(string key);

        /// <summary>
        /// Sets the entry.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="memoryCacheEntryOptions">The memory cache entry options.</param>
        void SetEntry<TEntity>(object key, object value, MemoryCacheEntryOptions memoryCacheEntryOptions);

        /// <summary>
        /// Gets the memory cache entry options.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <returns>The MemoryCacheEntryOptions.</returns>
        MemoryCacheEntryOptions GetMemoryCacheEntryOptions<TEntity>(CancellationTokenSource cancellationTokenSource);

        /// <summary>
        /// Validates the item identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        void ValidateItemId(string id);
    }
}
