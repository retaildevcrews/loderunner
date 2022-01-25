﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using LodeRunner.Core.Models;

namespace LodeRunner.API.Extensions
{
    /// <summary>
    /// Provides AutoMapper Profile Extension Methods.
    /// </summary>
    public static class AutoMapperProfileExtensions
    {
        /// <summary>
        /// Builds the expression ignoring the unmodified properties.
        /// </summary>
        /// <typeparam name="TPayloadSource">The type of the payload source.</typeparam>
        /// <typeparam name="TDestination">The type of the destination.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns>IMappingExpression</returns>
        public static IMappingExpression<TPayloadSource, TDestination> IgnoreUnmodifiedProperties<TPayloadSource, TDestination>(this IMappingExpression<TPayloadSource, TDestination> expression)
            where TPayloadSource : BasePayload
            where TDestination : BaseEntityModel
        {
            var destType = typeof(TDestination);
            foreach (var property in destType.GetProperties())
            {
                expression.ForMember(property.Name, opt => opt.PreCondition((srcPayload) =>
                {
                    if (srcPayload.PropertiesChanged.Contains(property.Name))
                    {
                        return true;
                    }

                    return false;
                }));
            }

            return expression;
        }
    }
}
