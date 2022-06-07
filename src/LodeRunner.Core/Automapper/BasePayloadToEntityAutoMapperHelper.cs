// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using AutoMapper;
using LodeRunner.Core.Models;

namespace LodeRunner.Core.Automapper
{
    /// <summary>
    /// Base Payload to Entity AutoMapper Helper.
    /// </summary>
    /// <typeparam name="TPayloadSource">The type of the source.</typeparam>
    /// <typeparam name="TDestination">The type of the destination.</typeparam>
    public static class BasePayloadToEntityAutoMapperHelper<TPayloadSource, TDestination>
        where TPayloadSource : BasePayload
        where TDestination : BaseEntityModel
    {
        private static readonly Mapper OneToOneMapper = new(new MapperConfiguration(cfg => cfg.CreateMap<TPayloadSource, TDestination>()));

        /// <summary>
        /// Maps the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>The new destination object.</returns>
        public static TDestination Map(TPayloadSource source)
        {
            return OneToOneMapper.Map<TDestination>(source);
        }
    }
}
