// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using LodeRunner.Core.Extensions;
using LodeRunner.Core.Models;

namespace LodeRunner.API.Extensions
{
    /// <summary>
    /// Provides AutoMapper Profile Extension Methods.
    /// </summary>
    public static class AutoMapperProfileExtensions
    {
        /// <summary>
        /// Builds the expression ignoring the null fields.
        /// </summary>
        /// <typeparam name="TPayloadSource">The type of the payload source.</typeparam>
        /// <typeparam name="TDestination">The type of the destination.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns>IMappingExpression</returns>
        public static IMappingExpression<TPayloadSource, TDestination> IgnoreNullFields<TPayloadSource, TDestination>(this IMappingExpression<TPayloadSource, TDestination> expression)
            where TPayloadSource : BasePayload
            where TDestination : BaseEntityModel
        {
            var destType = typeof(TDestination);
            foreach (var property in destType.GetProperties())
            {
                expression.ForMember(property.Name, opt => opt.Condition((srcPayload) =>
                {
                    var fields = srcPayload.FieldValue(property.Name);
                    if (fields[0] != null)
                    {
                        // This return value is to fulfill opt.Condition((srcPayload)) check. Determines whether or not the property will be included in the return expression.
                        return true;
                    }

                    // This return value is to fulfill opt.Condition((srcPayload)) check. Determines whether or not the property will be included in the return expression.
                    return false;
                }));
            }

            return expression;
        }
    }
}
