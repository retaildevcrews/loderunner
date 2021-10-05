// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LodeRunner.API.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LodeRunner.API.Handlers.ExceptionMiddleware
{
    /// <summary>
    /// Represents the Extensions methods for GlobalException.
    /// </summary>
    public static class GlobalExceptionExtensions
    {
        /// <summary>
        /// Configures the custom exception middleware.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>IApplicationBuilder.</returns>
        public static IApplicationBuilder ConfigureCustomExceptionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalException>();
        }
    }
}
