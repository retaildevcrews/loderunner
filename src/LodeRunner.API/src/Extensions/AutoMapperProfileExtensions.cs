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
    public static class AutoMapperProfileExtensions
    {
        public static IMappingExpression<LoadTestConfigPayload, LoadTestConfig> IgnoreNotProvidedProps(
            this IMappingExpression<LoadTestConfigPayload, LoadTestConfig> expression)
        {
            var destType = typeof(LoadTestConfig);
            foreach (var property in destType.GetProperties())
            {
                expression.ForMember(property.Name, opt => opt.Condition(srcPayload =>
                {
                    if (srcPayload != null && srcPayload.PropertiesChanged.Contains(property.Name))
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
