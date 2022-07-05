// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace LodeRunner.API.Middleware
{
    /// <summary>
    /// Middleware extension to make registering Logger easy
    ///
    /// Note: Logger should be one of the first things registered in DI.
    /// </summary>
    public static class RequestLoggerExtensions
    {
        /// <summary>
        /// Adds RequestLogger to middleware.
        /// </summary>
        /// <param name="builder">Application builder.</param>
        /// <param name="config">Config.</param>
        /// <param name="options">Request logger options.</param>
        /// <returns>Application <paramref name="builder"/>.</returns>
        public static IApplicationBuilder UseRequestLogger(this IApplicationBuilder builder, Config config, RequestLoggerOptions options = null)
        {
            // extension - use app.UseRequestLogger();
            if (options == null)
            {
                options = new RequestLoggerOptions();
            }

            object[] args = { Options.Create<RequestLoggerOptions>(options), config };

            return builder.UseMiddleware<RequestLogger>(args);
        }
    }
}
