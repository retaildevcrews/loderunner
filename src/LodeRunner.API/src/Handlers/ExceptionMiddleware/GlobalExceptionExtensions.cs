// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Builder;

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
