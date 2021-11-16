// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using LodeRunner.Core.SchemaFilters;
using Swashbuckle.AspNetCore.Annotations;

namespace LodeRunner.Core.Models
{
    /// <summary>
    /// Represents the IetfHealthCheck class required to provide a response example.
    /// </summary>
    [SwaggerSchemaFilter(typeof(IetfHealthCheckSchemaFilter))]
    public class IetfHealthCheck
    {
    }
}
