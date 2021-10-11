// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net;
using System.Threading.Tasks;
using LodeRunner.API.Middleware;
using Microsoft.AspNetCore.Http;

namespace LodeRunner.API.Handlers.ExceptionMiddleware
{
    /// <summary>
    /// Represent the Global Exception class.
    /// </summary>
    public class GlobalException
    {
        private readonly RequestDelegate next;
        private readonly NgsaLog logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalException"/> class.
        /// </summary>
        /// <param name="next">The next.</param>
        /// <param name="logger">The logger.</param>
        public GlobalException(RequestDelegate next, NgsaLog logger)
        {
            this.logger = logger;
            this.next = next;
        }

        /// <summary>
        /// Invokes the asynchronous.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <returns>The Task to Invoke next Async.</returns>
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await this.next(httpContext);
            }
            catch (Exception ex)
            {
                this.logger.LogError("Global Exception", $"{ex.Message}", ex: ex);
                await this.HandleExceptionAsync(httpContext, ex);
            }
        }

        /// <summary>
        /// Handles the exception asynchronous.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="exception">The exception.</param>
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "text/plain";

            string errorMessage;

            if (exception.GetType() == typeof(OperationCanceledException))
            {
                context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;

                errorMessage = $"{SystemConstants.Terminating} - {SystemConstants.TerminationDescription}";

                this.logger.LogError("GlobalException: HandleExceptionAsync", errorMessage);
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                errorMessage = $"{HttpStatusCode.InternalServerError} - {exception.Message}";

                this.logger.LogError("GlobalException: HandleExceptionAsync", $"Internal Server Error: {exception}");
            }

            await context.Response.WriteAsync(errorMessage);
        }
    }
}
