// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LodeRunner.API.Core
{
    /// <summary>
    /// Represents the Custom Path Prefix Insert class.
    /// </summary>
    public class PathPrefixInsertDocumentFilter : IDocumentFilter
    {
        private readonly string pathPrefix;

        /// <summary>
        /// Initializes a new instance of the <see cref="PathPrefixInsertDocumentFilter"/> class.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        public PathPrefixInsertDocumentFilter(string prefix)
        {
            this.pathPrefix = prefix;
        }

        /// <summary>
        /// Applies the specified swagger document.
        /// </summary>
        /// <param name="swaggerDoc">The swagger document.</param>
        /// <param name="context">The context.</param>
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var paths = swaggerDoc.Paths.Keys.ToList();
            foreach (var path in paths)
            {
                var pathToChange = swaggerDoc.Paths[path];
                swaggerDoc.Paths.Remove(path);
                swaggerDoc.Paths.Add($"{this.pathPrefix}{path}", pathToChange);
            }
        }
    }
}
