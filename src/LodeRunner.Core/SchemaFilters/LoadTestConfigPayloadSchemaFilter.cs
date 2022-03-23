// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LodeRunner.Core.SchemaFilters
{
    /// <summary>
    /// Defines the LoadTestConfig Schema.
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.SwaggerGen.ISchemaFilter" />
    public class LoadTestConfigPayloadSchemaFilter : ISchemaFilter
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
                ["name"] = new OpenApiString("Sample - LoadTestConfig"),
                ["files"] = new OpenApiArray
                            {
                                new OpenApiString("baseline.json"),
                                new OpenApiString("benchmark.json"),
                            },
                ["strictJson"] = new OpenApiBoolean(true),
                ["baseURL"] = new OpenApiString("Sample BaseURL"),
                ["verboseErrors"] = new OpenApiBoolean(true),
                ["randomize"] = new OpenApiBoolean(false),
                ["timeout"] = new OpenApiInteger(10),
                ["server"] = new OpenApiArray
                            {
                                new OpenApiString("www.yourprimaryserver.com"),
                                new OpenApiString("www.yoursecondaryserver.com"),
                            },
                ["tag"] = new OpenApiString("Sample Tag"),
                ["sleep"] = new OpenApiInteger(5),
                ["runLoop"] = new OpenApiBoolean(false),
                ["duration"] = new OpenApiInteger(60),
                ["maxErrors"] = new OpenApiInteger(10),
                ["dryRun"] = new OpenApiBoolean(false),
            };
        }
    }
}
