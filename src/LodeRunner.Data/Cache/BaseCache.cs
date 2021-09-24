// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Caching;
using System.Threading;
using LodeRunner.Data.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using MemoryCache = Microsoft.Extensions.Caching.Memory.MemoryCache;

namespace LodeRunner.Data.Cache
{
    /// <summary>
    /// Represents the Base Cache class implementation.
    /// </summary>
    /// <seealso cref="LodeRunner.Data.Interfaces.ICache" />
    public abstract class BaseCache : ICache
    {
        private const string BaseMemCacheInstanceName = "MemCache";

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCache"/> class.
        /// </summary>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        public BaseCache(CancellationTokenSource cancellationTokenSource)
        {
            this.CancellationTokenSource = cancellationTokenSource;
            this.CacheName = $"{BaseMemCacheInstanceName}-{Guid.NewGuid()}";
            this.Cache = new MemoryCache(new MemoryCacheOptions());
        }

        /// <summary>
        /// Gets the name of the cache.
        /// </summary>
        /// <value>
        /// The name of the cache.
        /// </value>
        public string CacheName { get; private set; }

        /// <summary>
        /// Gets the cancellation token source.
        /// </summary>
        /// <value>
        /// The cancellation token source.
        /// </value>
        protected CancellationTokenSource CancellationTokenSource { get; private set; }

        /// <summary>
        /// Gets the memory cache.
        /// </summary>
        /// <value>
        /// The memory cache.
        /// </value>
        protected MemoryCache Cache { get; private set; }

        /// <summary>
        /// Gets the entries.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="keyPrefix">The key prefix.</param>
        /// <returns>
        /// The Entries as Enumerable.
        /// </returns>
        public IEnumerable<TEntity> GetEntries<TEntity>(string keyPrefix)
        {
            // TODO:  use getenumerator to get all .

            // NOTE: Retrieving an enumerator for a MemoryCache instance is a resource - intensive and blocking operation.Therefore, the enumerator should not be used in production applications.
            List<string> keyList = (List<string>)this.Cache.Get(keyPrefix);
            List<TEntity> entryList = new ();
            foreach (string itemId in keyList)
            {
                string itemKey = $"{keyPrefix}-{itemId}";
                TEntity cacheEntry = (TEntity)this.Cache.Get(itemKey);
                if (cacheEntry != null)
                {
                    entryList.Add(cacheEntry);
                }
            }

            return entryList;
        }

        /// <summary>
        /// Sets the entries.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="TFlattenEntity">The type of the flatten entity.</typeparam>
        /// <param name="items">The items.</param>
        /// <param name="keyPrefix">The key prefix.</param>
        /// <param name="entityIdFieldName">Name of the entity Id Field.</param>
        public void SetEntries<TEntity, TFlattenEntity>(List<TEntity> items, string keyPrefix, string entityIdFieldName = "Id")
        {
            List<string> tEntityIds = new ();

            foreach (TEntity item in items)
            {
                string itemId = this.GetFieldValueAsString(this.GetFieldValue(item, entityIdFieldName));

                this.ValidateItemId(itemId);

                tEntityIds.Add(itemId);

                string itemKey = $"{keyPrefix}-{itemId}";

                // this.Cache.Set(keyPrefix, tEntityIds, new CacheItemPolicy());
                this.Cache.Set(keyPrefix, tEntityIds, new MemoryCacheEntryOptions());

                var flattentObject = Activator.CreateInstance(typeof(TFlattenEntity), item);

                // TODO: Need to validate clientStatus before to create Client ?
                // this.Cache.Set(itemKey, flattentObject, this.GetClientCachePolicy());
                this.Cache.Set(itemKey, flattentObject, this.GetMemoryCacheEntryOptions(this.CancellationTokenSource));
            }
        }

        /// <summary>
        /// Gets the entry by key.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="keyPrefix">The key prefix.</param>
        /// <param name="key">The key.</param>
        /// <returns>
        /// Cache Entry As TEntity type.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Invalid key.</exception>
        public TEntity GetEntryByKey<TEntity>(string keyPrefix, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            return (TEntity)this.Cache.Get($"{keyPrefix}-{key}");
        }

        /// <summary>
        /// Gets the entry by key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// The entry from the cache.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">if key is null or empty.</exception>
        public object GetEntry(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            return this.Cache.Get(key);
        }

        /// <summary>
        /// Sets the entry.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="memoryCacheEntryOptions">The memory cache entry options.</param>
        public void SetEntry(string key, object value, MemoryCacheEntryOptions memoryCacheEntryOptions)
        {
            this.Cache.Set(key, value, memoryCacheEntryOptions);
        }

        /// <summary>
        /// Gets the client cache policy.
        /// </summary>
        /// <returns>
        /// the CacheItemPolicy.
        /// </returns>
        public abstract CacheItemPolicy GetClientCachePolicy();

        /// <summary>
        /// Gets the memory cache entry options.
        /// </summary>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <returns>MemoryCacheEntryOptions.</returns>
        public abstract MemoryCacheEntryOptions GetMemoryCacheEntryOptions(CancellationTokenSource cancellationTokenSource);

        /// <summary>
        /// Validates the item identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public abstract void ValidateItemId(string id);

        /// <summary>
        /// Gets the field value as string.
        /// </summary>
        /// <param name="fieldValue">The field value.</param>
        /// <returns>Filed as string.</returns>
        private string GetFieldValueAsString(object fieldValue)
        {
            if (fieldValue != null)
            {
                return (string)fieldValue;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the field value.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="item">The item.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>the value of the field.</returns>
        private object GetFieldValue<TEntity>(TEntity item, string fieldName)
        {
            Type objectType = item.GetType();

            PropertyInfo fieldInfo = this.GetPropertyInfo(objectType, fieldName);

            if (fieldInfo != null)
            {
                return fieldInfo.GetValue(item);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the property information.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>PropertyInfo.</returns>
        private PropertyInfo GetPropertyInfo(Type type, string fieldName)
        {
            PropertyInfo fieldInfo;
            do
            {
                fieldInfo = type.GetProperty(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                type = type.BaseType;
            }
            while (fieldInfo == null && type != null);
            return fieldInfo;
        }
    }
}
