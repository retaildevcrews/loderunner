// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using LodeRunner.Core.Models;

namespace LodeRunner.API.Test.IntegrationTests.AutoMapper
{
    /// <summary>
    /// AutoMapper Extensions.
    /// </summary>
    public static class AutoMapperExtension
    {
        ///// <summary>
        ///// Automaps the sourceEntity and gets new instance of TEntity with a new Id.
        ///// </summary>
        ///// <typeparam name="TEntity">The type of the sub entity.</typeparam>
        ///// <param name="sourceEntity">The load client source.</param>
        ///// <returns>A new instance of TSubEntity with a different Id.</returns>
        //public static TEntity AutomapAndGetaNewEntity<TEntity>(this TEntity sourceEntity)
        //    where TEntity : BaseEntityModel
        //{
        //    // Do the mapping to assure we generate a new ID.
        //    return BaseEntityAutoMapperHelper<TEntity, TEntity>.MapIgnoringId(sourceEntity);
        //}

        /// <summary>
        /// Automaps the and get loadTestConfig payload.
        /// </summary>
        /// <param name="loadTestConfigSource">The loadTestConfigS source.</param>
        /// <returns>The Test Payload object.</returns>
        public static LoadTestConfigPayload AutomapAndGetLoadTestConfigTestPayload(this LoadTestConfig loadTestConfigSource)
        {
            //// Do the mapping to assure we use the payload class.
            return BasePayloadAutoMapperHelper<LoadTestConfig, LoadTestConfigPayload>.Map(loadTestConfigSource);
        }
    }
}
