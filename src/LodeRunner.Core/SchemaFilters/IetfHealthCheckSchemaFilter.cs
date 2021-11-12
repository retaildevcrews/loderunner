// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LodeRunner.Core.SchemaFilters
{
    /// <summary>
    /// Defines the IetfHealthCheck Schema.
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.SwaggerGen.ISchemaFilter" />
    public class IetfHealthCheckSchemaFilter : ISchemaFilter
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
                ["status"] = new OpenApiString("pass"),
                ["serviceId"] = new OpenApiString("LoadRunner.API"),
                ["description"] = new OpenApiString("LoadRunner.API Health Check"),
                ["instance"] = new OpenApiString("0"),
                ["version"] = new OpenApiString("1.0.0502.0026"),

                ["checks"] = new OpenApiArray
                            {
                                 new OpenApiObject
                                {
                                    ["status"] = new OpenApiString("pass"),
                                    ["componentId"] = new OpenApiString("getClientByClientStatusId"),
                                    ["componentType"] = new OpenApiString("datastore"),
                                    ["observedUnit"] = new OpenApiString("ms"),
                                    ["observedValue"] = new OpenApiString("1.4"),
                                    ["targetValue"] = new OpenApiString("400"),
                                    ["time"] = new OpenApiString("2021-05-02T10:25:11Z"),
                                },
                                 new OpenApiObject
                                {
                                    ["status"] = new OpenApiString("pass"),
                                    ["componentId"] = new OpenApiString("searchClient"),
                                    ["componentType"] = new OpenApiString("datastore"),
                                    ["observedUnit"] = new OpenApiString("ms"),
                                    ["observedValue"] = new OpenApiString("38.11"),
                                    ["targetValue"] = new OpenApiString("400"),
                                    ["time"] = new OpenApiString("2021-05-02T10:25:11Z"),
                                },
                            },
            };
        }
    }
}
