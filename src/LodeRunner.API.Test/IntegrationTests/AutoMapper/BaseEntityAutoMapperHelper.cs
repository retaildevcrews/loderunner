// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using AutoMapper;
using LodeRunner.Core.Interfaces;
using LodeRunner.Core.Models;

namespace LodeRunner.API.Test.IntegrationTests.AutoMapper
{
    /// <summary>
    /// BaseEntity AutoMapper Helper.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TDestination">The type of the destination.</typeparam>
    public static class BaseEntityAutoMapperHelper<TSource, TDestination>
        where TSource : BaseEntityModel
        where TDestination : BaseEntityModel
    {
        private static readonly Mapper OneToOneMapper = new (new MapperConfiguration(cfg => cfg.CreateMap<TSource, TDestination>()));
        private static readonly Mapper OneToOneMapperIgnoringId = new (new MapperConfiguration(cfg => cfg.CreateMap<TSource, TDestination>().ForMember(dest => dest.Id, opt => opt.Ignore())));

        /// <summary>
        /// Maps the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>The new destination object.</returns>
        public static TDestination Map(TSource source)
        {
            return OneToOneMapper.Map<TDestination>(source);
        }

        /// <summary>
        /// Maps the ignoring identifier.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>The new destination object.</returns>
        public static TDestination MapIgnoringId(TSource source)
        {
            return OneToOneMapperIgnoringId.Map<TDestination>(source);
        }
    }
}
