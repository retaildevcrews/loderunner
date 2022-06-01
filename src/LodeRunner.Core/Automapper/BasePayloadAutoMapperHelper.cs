// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using AutoMapper;
using LodeRunner.Core.Models;

namespace LodeRunner.Core.Automapper
{
    /// <summary>
    /// BasePayload AutoMapper Helper.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TPayloadDestination">The type of the destination.</typeparam>
    public static class BasePayloadAutoMapperHelper<TSource, TPayloadDestination>
        where TSource : BaseEntityModel
        where TPayloadDestination : BasePayload
    {
        private static readonly Mapper OneToOneMapper = new(new MapperConfiguration(cfg => cfg.CreateMap<TSource, TPayloadDestination>()));

        /// <summary>
        /// Maps the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>The new destination object.</returns>
        public static TPayloadDestination Map(TSource source)
        {
            return OneToOneMapper.Map<TPayloadDestination>(source);
        }
    }
}
