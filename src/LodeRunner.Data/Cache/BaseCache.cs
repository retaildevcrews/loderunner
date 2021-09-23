// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Caching;
using LodeRunner.Data.Interfaces;

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
        public BaseCache()
        {
            this.CacheName = $"{BaseMemCacheInstanceName}-{Guid.NewGuid()}";
            this.MemCache = new MemoryCache(this.CacheName);
        }

        /// <summary>
        /// Gets the name of the cache.
        /// </summary>
        /// <value>
        /// The name of the cache.
        /// </value>
        public string CacheName { get; private set; }

        /// <summary>
        /// Gets the memory cache.
        /// </summary>
        /// <value>
        /// The memory cache.
        /// </value>
        protected MemoryCache MemCache { get; private set; }

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
            List<string> keyList = (List<string>)this.MemCache.Get(keyPrefix);
            List<TEntity> entryList = new ();
            foreach (string itemId in keyList)
            {
                string itemKey = $"{keyPrefix}-{itemId}";
                TEntity cacheEntry = (TEntity)this.MemCache.Get(itemKey);
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
        /// <param name="idPropertyName">Name of the identifier property.</param>
        public void SetEntries<TEntity, TFlattenEntity>(List<TEntity> items, string keyPrefix, string idPropertyName = "id")
        {
            List<string> tEntityIds = new ();

            foreach (TEntity item in items)
            {
                string itemId = this.GetFieldValueAsString(this.GetFieldValue(item, idPropertyName));

                this.ValidateItemId(itemId);

                tEntityIds.Add(itemId);

                string itemKey = $"{keyPrefix}-{itemId}";

                this.MemCache.Set(keyPrefix, tEntityIds, new CacheItemPolicy());

                var flattentObject = Activator.CreateInstance(typeof(TFlattenEntity), item);

                // TODO: Need to validate clientStatus before to create Client ?
                this.MemCache.Set(itemKey, flattentObject, this.GetClientCachePolicy());
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

            return (TEntity)this.MemCache.Get($"{keyPrefix}-{key}");
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

            return this.MemCache.Get(key);
        }

        /// <summary>
        /// Sets the entry.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="policy">The policy.</param>
        /// <param name="regionName">Name of the region.</param>
        public void SetEntry(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            this.MemCache.Set(key, value, policy, regionName);
        }

        /// <summary>
        /// Gets the client cache policy.
        /// </summary>
        /// <returns>
        /// the CacheItemPolicy.
        /// </returns>
        public abstract CacheItemPolicy GetClientCachePolicy();

        /// <summary>
        /// Validates the item identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public abstract void ValidateItemId(string id);

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
                return fieldInfo.GetValue(this);
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
