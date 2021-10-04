// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LodeRunner.Core.Models;
using Microsoft.Extensions.Caching.Memory;

namespace LodeRunner.Core.Cache
{
    /// <summary>
    /// The Extensions Memory Base Class.
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Caching.Memory.MemoryCache" />
    internal class BaseMemoryCache : MemoryCache
    {
        /// <summary>
        /// The get entries collection.
        /// </summary>
        private static readonly Func<MemoryCache, object> GetEntriesCollection = Delegate.CreateDelegate(
             typeof(Func<MemoryCache, object>),
             typeof(MemoryCache).GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true),
             throwOnBindFailure: true) as Func<MemoryCache, object>;

        private readonly EntityType entityType;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseMemoryCache"/> class.
        /// </summary>
        /// <param name="memoryOption">The memory option.</param>
        /// <param name="entityType">Type of the entity.</param>
        public BaseMemoryCache(MemoryCacheOptions memoryOption, EntityType entityType)
            : base(memoryOption)
        {
            this.entityType = entityType;

            this.CacheName = this.entityType.ToString();
        }

        /// <summary>
        /// Gets the name of the cache.
        /// </summary>
        /// <value>
        /// The name of the cache.
        /// </value>
        public string CacheName { get; private set; }

        /// <summary>
        /// Gets the keys.
        /// </summary>
        /// <returns>Cache Keys Enumerable.</returns>
        public IEnumerable GetKeys() => ((IDictionary)GetEntriesCollection(this)).Keys;

        /// <summary>
        /// Gets the keys.
        /// </summary>
        /// <typeparam name="T">Key Type.</typeparam>
        /// <returns>Cache Keys Enumerable.</returns>
        public IEnumerable<T> GetKeys<T>() => this.GetKeys().OfType<T>();
    }
}
