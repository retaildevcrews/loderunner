// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using LodeRunner.Core.Models;

namespace LodeRunner.Core.Automapper
{
    /// <summary>
    /// AutoMapper Extensions.
    /// </summary>
    public static class AutoMapperExtension
    {
        /// <summary>
        /// Create a new instance of TEntity with same Id.
        /// </summary>
        /// <typeparam name="TEntity">The type of the sub entity.</typeparam>
        /// <param name="sourceEntity">The load client source.</param>
        /// <returns>A new instance of TEntity with same Id.</returns>
        public static TEntity Clone<TEntity>(this TEntity sourceEntity)
            where TEntity : BaseEntityModel
        {
            return BaseEntityAutoMapperHelper<TEntity, TEntity>.Map(sourceEntity);
        }

        /// <summary>
        /// Creates a new instance of TEntity with a new Id.
        /// </summary>
        /// <typeparam name="TEntity">The type of the sub entity.</typeparam>
        /// <param name="sourceEntity">The entity source.</param>
        /// <returns>A new instance of TEntity with a new Id.</returns>
        public static TEntity CopyToNewInstance<TEntity>(this TEntity sourceEntity)
            where TEntity : BaseEntityModel
        {
            // Do the mapping to assure we generate a new ID.
            return BaseEntityAutoMapperHelper<TEntity, TEntity>.MapIgnoringId(sourceEntity);
        }

        /// <summary>
        /// Creates a new LoadTestConfigPayload instance from LoadTestConfig.
        /// </summary>
        /// <param name="loadTestConfigSource">The loadTestConfig source.</param>
        /// <returns>The Test Payload object.</returns>
        public static LoadTestConfigPayload MapLoadTestConfigToPayload(this LoadTestConfig loadTestConfigSource)
        {
            // Do the mapping to assure we use the payload class.
            return BasePayloadAutoMapperHelper<LoadTestConfig, LoadTestConfigPayload>.Map(loadTestConfigSource);
        }
    }
}
