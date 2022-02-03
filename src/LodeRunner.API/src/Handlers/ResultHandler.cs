// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LodeRunner.API.Middleware.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace LodeRunner.API.Middleware
{
    /// <summary>
    /// Handles query requests from the controllers.
    /// </summary>
    public static class ResultHandler
    {
        private const string JsonContentTypeApplicationJson = "application/json";
        private const string JsonContentTypeApplicationJsonProblem = "application/problem+json";

        private const string DataRequest = "Data request.";
        private const string DataNotFound = "Requested data not found.";

        /// <summary>
        /// Creates the response for GET (all) methods.
        /// </summary>
        /// <typeparam name="TEntity">Model entity.</typeparam>
        /// <param name="getResult">Async task to retrieve results from data storage.</param>
        /// <param name="logger">The Logger.</param>
        /// <param name="methodName">Caller member name to improve logging.</param>
        /// <returns>A task with the appropriate response.</returns>
        public static async Task<ActionResult> CreateGetResponse<TEntity>(Func<Task<IEnumerable<TEntity>>> getResult, ILogger logger, [CallerMemberName] string methodName = null)
        {
            try
            {
                var result = await getResult();

                if (!(result as IEnumerable<object>).Any())
                {
                    return new NoContentResult();
                }

                return new OkObjectResult(result);
            }
            catch (CosmosException ce)
            {
                if (ce.StatusCode == HttpStatusCode.NotFound)
                {
                    // Log Warning
                    logger.LogWarning(new EventId((int)ce.StatusCode, $"{methodName} > {nameof(CreateGetResponse)}"), $"{SystemConstants.NotFoundError}");

                    return new NoContentResult();
                }
                else
                {
                    // Log Error
                    logger.LogError(new EventId((int)HttpStatusCode.BadRequest, $"{methodName} > {nameof(CreateGetResponse)}"), ce, "CosmosException");
                    return CreateInternalServerErrorResponse($"{methodName} > {nameof(CreateGetResponse)} > CosmosException > [{ce.StatusCode}] {ce.Message}");
                }
            }
            catch (Exception ex)
            {
                // Log Error
                logger.LogError(new EventId((int)HttpStatusCode.InternalServerError, $"{methodName} > {nameof(CreateGetResponse)}"), ex, "Exception");
                return CreateInternalServerErrorResponse($"{methodName} > {nameof(CreateGetResponse)} > {ex.Message}");
            }
        }

        /// <summary>
        /// Creates the response for ServiceUnavailable.
        /// Currently only handles Cancellation InProgress Result use case.
        /// </summary>
        /// <returns>JsonResult.</returns>
        public static JsonResult CreateServiceUnavailableResponse()
        {
            return new JsonResult(new ErrorResult { Error = HttpStatusCode.ServiceUnavailable, Message = $"{SystemConstants.Terminating} - {SystemConstants.TerminationDescription}" })
            {
                StatusCode = (int)HttpStatusCode.ServiceUnavailable,
                ContentType = JsonContentTypeApplicationJson,
            };
        }

        /// <summary>
        /// Creates No Content Result.
        /// </summary>
        /// <returns>JsonResult.</returns>
        public static async Task<NoContentResult> CreateNoContent()
        {
            return await Task.Run(() => new NoContentResult());
        }

        /// <summary>
        /// Creates an Error JsonResult.
        /// </summary>
        /// <param name="message">The Message.</param>
        /// <param name="statusCode">The Message StatusCode.</param>
        /// <returns>JsonResult.</returns>
        public static async Task<JsonResult> CreateErrorResult(string message, HttpStatusCode statusCode)
        {
            return await CreateResult(new ErrorResult { Error = statusCode, Message = message }, statusCode);
        }

        /// <summary>
        /// Content Result from data.
        /// </summary>
        /// <typeparam name="TEntity">the data type.</typeparam>
        /// <param name="data">the data.</param>
        /// <param name="statusCode">The http code.</param>
        /// <param name="contentType">Json Content Type.</param>
        /// <returns>the Json Result.</returns>
        public static async Task<JsonResult> CreateResult<TEntity>(TEntity data, HttpStatusCode statusCode, string contentType = JsonContentTypeApplicationJson)
        {
            return await Task.Run(() =>
            {
                JsonResult res = new (data)
                {
                    StatusCode = (int)statusCode,
                    ContentType = contentType,
                };

                return res;
            });
        }

        /// <summary>
        /// Create a BadRequest Result.
        /// </summary>
        /// <param name="errors">list of validation errors.</param>
        /// <param name="path">string.</param>
        /// <returns>JsonResult.</returns>
        public static async Task<JsonResult> CreateBadRequestResult(object errors, string path)
        {
            Dictionary<string, object> data = new ()
            {
                { "type", ValidationError.GetErrorLink(path) },
                { "title", "Parameter validation error" },
                { "detail", "One or more invalid parameters were specified." },
                { "status", (int)HttpStatusCode.BadRequest },
                { "instance", path },
                { "validationErrors", errors },
            };

            return await CreateResult(data, HttpStatusCode.BadRequest, JsonContentTypeApplicationJsonProblem);
        }

        /// <summary>
        /// Handles the result.
        /// </summary>
        /// <param name="results">The results.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>The Action Result.</returns>
        public static async Task<ActionResult> HandleResult(object results, ILogger logger)
        {
            if (results == null)
            {
                return await CreateErrorResult(DataNotFound, HttpStatusCode.NotFound);
            }
            else if (results is IEnumerable<object> && !(results as IEnumerable<object>).Any())
            {
                return await CreateNoContent();
            }
            else
            {
                try
                {
                    // return an OK object result
                    return new OkObjectResult(results);
                }
                catch (Exception ex)
                {
                    // log and return exception
                    logger.LogError(new EventId((int)(int)HttpStatusCode.InternalServerError, nameof(HandleResult)), ex, "Exception");

                    // return 500 error
                    return await CreateErrorResult("Internal Server Error", HttpStatusCode.InternalServerError);
                }
            }
        }

        /// <summary>
        /// Create response for internal server error.
        /// </summary>
        /// <param name="message">Message to include in the response.</param>
        /// <returns>JsonResult.</returns>
        private static JsonResult CreateInternalServerErrorResponse(string message)
        {
            return new JsonResult(new ErrorResult { Error = HttpStatusCode.InternalServerError, Message = $"Internal Server Error: {message}" })
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                ContentType = JsonContentTypeApplicationJson,
            };
        }
    }
}
