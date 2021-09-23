// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Runtime.Caching;

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
        /// <param name="idPropertyName">Name of the identifier property.</param>
        void SetEntries<TEntity, TFlattenEntity>(List<TEntity> items, string keyPrefix, string idPropertyName = "id");

        /// <summary>
        /// Gets the entry by key.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="keyPrefix">The key prefix.</param>
        /// <param name="key">The key.</param>
        /// <returns>Cache Entry As TEntity type.</returns>
        TEntity GetEntryByKey<TEntity>(string keyPrefix, string key);

        /// <summary>
        /// Gets the entry by key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The entry from the cache.</returns>
        object GetEntry(string key);

        /// <summary>
        /// Sets the entry.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="policy">The policy.</param>
        /// <param name="regionName">Name of the region.</param>
        void SetEntry(string key, object value, CacheItemPolicy policy, string regionName = null);

        /// <summary>
        /// Gets the client cache policy.
        /// </summary>
        /// <returns> the CacheItemPolicy.</returns>
        CacheItemPolicy GetClientCachePolicy();

        /// <summary>
        /// Validates the item identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        void ValidateItemId(string id);
    }
}
