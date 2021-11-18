// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using LodeRunner.API.Middleware.Validation;
using Microsoft.AspNetCore.Mvc;

namespace LodeRunner.API.Middleware
{
    /// <summary>
    /// Handles query requests from the controllers.
    /// </summary>
    public static class ResultHandler
    {
        /// <summary>
        /// ContentResult factory.
        /// </summary>
        /// <param name="message">The Message.</param>
        /// <param name="statusCode">The Message StatusCode.</param>
        /// <returns>JsonResult.</returns>
        public static async Task<JsonResult> CreateResult(string message, HttpStatusCode statusCode)
        {
           return await Task.Run(() =>
           {
               JsonResult res = new (new ErrorResult { Error = statusCode, Message = message })
               {
                   StatusCode = (int)statusCode,
               };

               return res;
           });
        }

        /// <summary>
        /// Content Result from data.
        /// </summary>
        /// <typeparam name="TEntity">the data type</typeparam>
        /// <param name="data">the data.</param>
        /// <param name="statusCode">The http code.</param>
        /// <returns>the Json Result</returns>
        public static async Task<JsonResult> CreateResult<TEntity>(TEntity data, HttpStatusCode statusCode)
        {
            return await Task.Run(() =>
            {
                JsonResult res = new (data)
                {
                    StatusCode = (int)statusCode,
                };

                return res;
            });
        }

        /// <summary>
        /// ContentResult factory.
        /// Creates Cancellation InProgress Result.
        /// </summary>
        /// <returns>JsonResult.</returns>
        public static async Task<JsonResult> CreateCancellationInProgressResult()
        {
            return await CreateResult($"{SystemConstants.Terminating} - {SystemConstants.TerminationDescription}", HttpStatusCode.ServiceUnavailable);
        }

        /// <summary>
        /// ContentResult factory.
        /// </summary>
        /// <param name="errorList">list of validation errors.</param>
        /// <param name="path">string.</param>
        /// <returns>JsonResult.</returns>
        public static async Task<JsonResult> CreateResult(List<ValidationError> errorList, string path)
        {
            return await Task.Run(() =>
                {
                    Dictionary<string, object> data = new ()
                    {
                        { "type", ValidationError.GetErrorLink(path) },
                        { "title", "Parameter validation error" },
                        { "detail", "One or more invalid parameters were specified." },
                        { "status", (int)HttpStatusCode.BadRequest },
                        { "instance", path },
                        { "validationErrors", errorList },
                    };

                    JsonResult res = new (data)
                    {
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        ContentType = "application/problem+json",
                    };

                    return res;
                });
        }
    }
}
