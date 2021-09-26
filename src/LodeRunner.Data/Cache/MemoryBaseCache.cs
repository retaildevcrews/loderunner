// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using LodeRunner.Core.Models;
using Microsoft.Extensions.Caching.Memory;

namespace LodeRunner.Data.Cache
{
    /// <summary>
    /// The Extensions Memory Base Class.
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Caching.Memory.MemoryCache" />
    internal class MemoryBaseCache : MemoryCache
    {
        private readonly EntityType entityType;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryBaseCache"/> class.
        /// </summary>
        /// <param name="memoryOption">The memory option.</param>
        /// <param name="entityType">Type of the entity.</param>
        public MemoryBaseCache(MemoryCacheOptions memoryOption, EntityType entityType)
            : base(memoryOption)
        {
            this.entityType = entityType;
        }

        /// <summary>
        /// Gets the name of the cache.
        /// </summary>
        /// <value>
        /// The name of the cache.
        /// </value>
        public string CacheName
        {
            get
            {
                return this.entityType.ToString();
            }
        }
    }
}
