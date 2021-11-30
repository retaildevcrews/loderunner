// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.Core.Extensions;
using LodeRunner.Core.Interfaces;
using LodeRunner.Core.Models;
using Microsoft.Extensions.Caching.Memory;

namespace LodeRunner.Core.Cache
{
    /// <summary>
    /// Represents the Base Cache class implementation.
    /// </summary>
    public abstract class BaseAppCache : IAppCache
    {
        private readonly ConcurrentDictionary<string, BaseMemoryCache> cacheDictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseAppCache"/> class.
        /// </summary>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        public BaseAppCache(CancellationTokenSource cancellationTokenSource)
        {
            this.CancellationTokenSource = cancellationTokenSource;
            this.cacheDictionary = new ();
        }

        /// <summary>
        /// Gets the cancellation token source.
        /// </summary>
        /// <value>
        /// The cancellation token source.
        /// </value>
        protected CancellationTokenSource CancellationTokenSource { get; private set; }

        /// <summary>
        /// Gets the entries.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns>
        /// The Entries as Enumerable.
        /// </returns>
        public async Task<IEnumerable<TEntity>> GetEntries<TEntity>()
        {
            return await Task.Run(() =>
            {
                BaseMemoryCache entityCache = this.GetMemCache<TEntity>();

                List<TEntity> entryList = new ();

                foreach (string itemKey in entityCache.GetKeys())
                {
                    TEntity cacheEntry = entityCache.Get<TEntity>(itemKey);
                    if (cacheEntry != null)
                    {
                        entryList.Add(cacheEntry);
                    }
                }

                return entryList;
            });
        }

        /// <summary>
        /// Gets the entry.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>the cache entry.</returns>
        public async Task<TEntity> GetEntry<TEntity>(object key)
        {
            return await Task.Run(() =>
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                BaseMemoryCache entityCache = this.GetMemCache<TEntity>();

                return entityCache.Get<TEntity>(key);
            });
        }

        /// <summary>
        /// Sets the entry.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="memoryCacheEntryOptions">The memory cache entry options.</param>
        /// <returns>The Task.</returns>
        public Task SetEntry<TEntity>(object key, TEntity value, MemoryCacheEntryOptions memoryCacheEntryOptions)
        {
            return Task.Run(() =>
            {
                BaseMemoryCache entityCache = this.GetMemCache<TEntity>();

                entityCache.Set(key, value, memoryCacheEntryOptions);
            });
        }

        /// <summary>
        /// Creates or Gets the typed cache.
        /// </summary>
        /// <typeparam name="TEntity">The cache entity type.</typeparam>
        /// <returns>the typed cache.</returns>
        private BaseMemoryCache GetMemCache<TEntity>()
        {
            this.CancellationTokenSource.Token.ThrowIfCancellationRequested();

            EntityType entityType = typeof(TEntity).Name.As<EntityType>();

            if (!this.cacheDictionary.TryGetValue(entityType.ToString(), out BaseMemoryCache thisCache))
            {
                thisCache = new (new MemoryCacheOptions(), entityType);
                this.cacheDictionary.TryAdd(entityType.ToString(), thisCache);
            }

            return thisCache;
        }
    }
}
