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
                await next(httpContext);
            }
            catch (Exception ex)
            {
                logger.LogError("Global Exception", $"{ex.Message}", ex: ex);
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        /// <summary>
        /// Handles the exception asynchronous.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="exception">The exception.</param>
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            string errorMessage = HttpStatusCode.InternalServerError.ToString();
            int statusCode = context.Response.StatusCode;

            if (exception.GetType() == typeof(OperationCanceledException))
            {
                errorMessage = $"{SystemConstants.Terminating} - {SystemConstants.TerminationDescription}";
                statusCode = (int)HttpStatusCode.ServiceUnavailable;
                context.Response.StatusCode = statusCode;
                this.logger.LogError("Middleware HandleExceptionAsync", errorMessage);
            }
            else
            {
                this.logger.LogError("Middleware HandleExceptionAsync", $"Internal Server Error: {exception}");
            }

            await context.Response.WriteAsync(new ErrorDetails()
            {
                StatusCode = statusCode,
                Message = errorMessage,
            }.ToString());
        }
    }
}
