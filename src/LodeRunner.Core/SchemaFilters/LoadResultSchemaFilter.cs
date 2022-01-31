// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LodeRunner.Core.SchemaFilters
{
    /// <summary>
    /// Defines the LoadResult Schema.
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.SwaggerGen.ISchemaFilter" />
    public class LoadResultSchemaFilter : ISchemaFilter
    {
        /// <summary>
        /// Applies the specified schema.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="context">The context.</param>
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            schema.Example = new OpenApiObject
            {
                ["loadClient"] = new OpenApiObject
                {
                    ["id"] = new OpenApiString("f7abf4f7-c1a4-4720-bda7-9251276b21f7"),
                    ["version"] = new OpenApiString("1.0.1"),
                    ["region"] = new OpenApiString("Central"),
                    ["zone"] = new OpenApiString("central-az-1"),
                    ["prometheus"] = new OpenApiBoolean(false),
                    ["startupArgs"] = new OpenApiString("--mode Client --region Central --zone central-az-1 --prometheus true"),
                    ["startTime"] = new OpenApiDateTime(new DateTime(2021, 12, 30, 2, 3, 4)),
                },
                ["startTime"] = new OpenApiDateTime(new DateTime(2022, 1, 7, 0, 30, 0)),
                ["completedTime"] = new OpenApiDateTime(new DateTime(2022, 1, 7, 1, 15, 0)),
                ["totalRequests"] = new OpenApiInteger(11),
                ["successfulRequests"] = new OpenApiInteger(1),
                ["failedRequests"] = new OpenApiInteger(10),
            };
        }
    }
}
