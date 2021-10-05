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

        //public static void ConfigureExceptionHandler(this IApplicationBuilder app, NgsaLog logger)
        //{
        //    app.UseExceptionHandler(appError =>
        //    {
        //        appError.Run(async context =>
        //        {
        //            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        //            context.Response.ContentType = "application/json";
        //            var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
        //            if (contextFeature != null)
        //            {
        //                string errorMessage = "Internal Server Error.";
        //                int statusCode = context.Response.StatusCode;

        //                if (contextFeature.Error.GetType() == typeof(OperationCanceledException))
        //                {
        //                    errorMessage = $"{SystemConstants.Terminating} - {SystemConstants.TerminationDescription}";
        //                    statusCode = (int)HttpStatusCode.ServiceUnavailable;
        //                    context.Response.StatusCode = statusCode;
        //                    logger.LogError("ConfigureExceptionHandler", errorMessage);
        //                }
        //                else
        //                {
        //                    logger.LogError("ConfigureExceptionHandler", $"Internal Server Error: {contextFeature.Error}");
        //                }

        //                await context.Response.WriteAsync(new ErrorDetails()
        //                {
        //                    StatusCode = statusCode,
        //                    Message = errorMessage,
        //                }.ToString());
        //            }
        //        });
        //    });
        //}
    }
}
