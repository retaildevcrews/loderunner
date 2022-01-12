// Copyright (c) Microsoft Corporation. All rights reserved.
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
        /// Builds the expression ignoring the not provided properties.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>IMappingExpression</returns>
        public static IMappingExpression<LoadTestConfigPayload, LoadTestConfig> IgnoreNotProvidedProps(
            this IMappingExpression<LoadTestConfigPayload, LoadTestConfig> expression)
        {
            var destType = typeof(LoadTestConfig);
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

        /// <summary>
        /// Builds the expression ignoring the not provided properties.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>IMappingExpression</returns>
        public static IMappingExpression<TestRunPayload, TestRun> IgnoreNotProvidedProps(
            this IMappingExpression<TestRunPayload, TestRun> expression)
        {
            var destType = typeof(TestRun);
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
