// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;

namespace LodeRunner
{
    /// <summary>
    /// Registers aspnet middleware handler that handles /version.
    /// </summary>
    public static class VersionExtension
    {
        // cached response
        private static byte[] responseBytes;

        /// <summary>
        /// Middleware extension method to handle /version request.
        /// </summary>
        /// <param name="builder">this IApplicationBuilder.</param>
        /// <returns>IApplicationBuilder.</returns>
        public static IApplicationBuilder UseVersion(this IApplicationBuilder builder)
        {
            // implement the middleware
            builder.Use(async (context, next) =>
            {
                // matches /version
                if (context.Request.Path.Value.Equals("/version", StringComparison.OrdinalIgnoreCase))
                {
                    // cache the version info for performance
                    if (responseBytes == null)
                    {
                        responseBytes = System.Text.Encoding.UTF8.GetBytes(Core.Version.AssemblyVersion);
                    }

                    // return the version info
                    context.Response.ContentType = "text/plain";
                    await context.Response.Body.WriteAsync(responseBytes).ConfigureAwait(false);
                }
                else
                {
                    // not a match, so call next middleware handler
                    await next().ConfigureAwait(false);
                }
            });

            return builder;
        }
    }
}
