﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using LodeRunner.API.Middleware.Validation;
using LodeRunner.Data.Interfaces;
using Microsoft.AspNetCore.Http;
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
        private const string JsonContentTypeApplicationJsonProblem = "application/problem+json";

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

                // No content response
                if (!(result as IEnumerable<object>).Any())
                {
                    return new NoContentResult();
                }

                // OK response
                return new OkObjectResult(result);
            }
            catch (CosmosException ce)
            {
                // Returns most common Exception: 404 NotFound, 429 TooManyReqs
                if (ce.StatusCode == HttpStatusCode.NotFound)
                {
                    // Log Warning
                    logger.LogWarning(new EventId((int)ce.StatusCode, $"{methodName} > {nameof(CreateGetResponse)}"), SystemConstants.NotFoundError);
                    return new NoContentResult();
                }

                // Log Error
                logger.LogError(new EventId((int)ce.StatusCode, $"{methodName} > {nameof(CreateGetResponse)}"), ce, "CosmosException");
                return CreateInternalServerErrorResponse($"{methodName} > {nameof(CreateGetResponse)} > CosmosException > [{ce.StatusCode}] {ce.Message}");
            }
            catch (Exception ex)
            {
                // Log Error
                logger.LogError(new EventId((int)HttpStatusCode.InternalServerError, $"{methodName} > {nameof(CreateGetResponse)}"), ex, "Exception");
                return CreateInternalServerErrorResponse($"{methodName} > {nameof(CreateGetResponse)} > {ex.Message}");
            }
        }

        /// <summary>
        /// Creates the response for GET by ID methods.
        /// </summary>
        /// <typeparam name="TEntity">Model entity.</typeparam>
        /// <param name="getResult">Gets result from data storage by ID.</param>
        /// <param name="id">TEntity ID to search by.</param>
        /// <param name="path">Request path.</param>
        /// <param name="errorList">List of ValidationErrors.</param>
        /// <param name="logger">The Logger.</param>
        /// <param name="methodName">Caller member name to improve logging.</param>
        /// <returns>A task with the appropriate response.</returns>
        public static async Task<ActionResult> CreateGetByIdResponse<TEntity>(Func<string, Task<TEntity>> getResult, string id, string path, IEnumerable<string> errorList, ILogger logger, [CallerMemberName] string methodName = null)
        {
            return await TryCatchException(logger, $"{methodName} > {nameof(CreateGetByIdResponse)}", async () =>
            {
                // Bad request response due to invalid ID
                if (errorList != null && errorList.Any())
                {
                    // Log Warning
                    logger.LogWarning(new EventId((int)HttpStatusCode.BadRequest, $"{methodName} > {nameof(CreateGetByIdResponse)}"), $"{SystemConstants.BadRequest}: {SystemConstants.InvalidParameter}, ID ({id})");

                    // Add info to response
                    return CreateValidationErrorResponse(SystemConstants.InvalidParameter, path, errorList);
                }

                var result = await getResult(id);

                // Not found response
                if (result == null)
                {
                    return new NotFoundResult();
                }

                // OK response
                return new OkObjectResult(result);
            });
        }

        public static async Task<ActionResult> TryCatchException(ILogger logger, string methodName, Func<Task<ActionResult>> taskToExecute)
        {
            try
            {
                return await taskToExecute();
            }
            catch (CosmosException ce)
            {
                // Returns most common Exception: 404 NotFound, 429 TooManyReqs
                if (ce.StatusCode == HttpStatusCode.NotFound)
                {
                    // Log Warning
                    logger.LogWarning(new EventId((int)ce.StatusCode, $"{methodName}"), SystemConstants.NotFoundError);
                    return new NotFoundResult();
                }

                // Log Error
                logger.LogError(new EventId((int)ce.StatusCode, $"{methodName}"), ce, "CosmosException");
                return CreateInternalServerErrorResponse($"{methodName} > CosmosException > [{ce.StatusCode}] {ce.Message}");
            }
            catch (Exception ex)
            {
                // Log Error
                logger.LogError(new EventId((int)HttpStatusCode.InternalServerError, $"{methodName} > {nameof(CreateGetByIdResponse)}"), ex, "Exception");
                return CreateInternalServerErrorResponse($"{methodName} > {ex.Message}");
            }
        }

        /// <summary>
        /// Creates the response for POST methods.
        /// </summary>
        /// <typeparam name="TEntity">Model entity.</typeparam>
        /// <param name="postResult">Posts payload to data storage.</param>
        /// <param name="payload">Payload.</param>
        /// <param name="path">Request path.</param>
        /// <param name="errorList">List of ValidationErrors.</param>
        /// <param name="logger">The Logger.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="methodName">Caller member name to improve logging.</param>
        /// <returns>A task with the appropriate response.</returns>
        public static async Task<ActionResult> CreatePostResponse<TEntity>(Func<TEntity, CancellationToken, Task<TEntity>> postResult, TEntity payload, string path, IEnumerable<string> errorList, ILogger logger, CancellationToken cancellationToken, [CallerMemberName] string methodName = null)
        {
            try
            {
                // Bad request response due to invalid payload
                if (errorList != null && errorList.Any())
                {
                    logger.LogWarning(new EventId((int)HttpStatusCode.BadRequest, $"{methodName} > {nameof(CreatePostResponse)}"), $"{SystemConstants.BadRequest}: {SystemConstants.InvalidPayload}");
                    return CreateValidationErrorResponse(SystemConstants.InvalidPayload, path, errorList);
                }

                var result = await postResult(payload, cancellationToken);

                // Internal server error response due to no returned value from storage create
                if (result == null)
                {
                    logger.LogError(new EventId((int)HttpStatusCode.InternalServerError, $"{methodName} > {nameof(CreatePostResponse)}"), null, SystemConstants.UpsertError);
                    return CreateInternalServerErrorResponse($"{methodName} > {nameof(CreatePostResponse)} > {SystemConstants.UpsertError}");
                }

                // Created response
                return new CreatedResult(path, result);
            }
            catch (CosmosException ce)
            {
                // Log Error
                logger.LogError(new EventId((int)ce.StatusCode, $"{methodName} > {nameof(CreatePostResponse)}"), ce, "CosmosException");

                // Returns most common Exception: 404 NotFound, 429 TooManyReqs
                return CreateInternalServerErrorResponse($"{methodName} > {nameof(CreatePostResponse)} > CosmosException > [{ce.StatusCode}] {ce.Message}");
            }
            catch (Exception ex)
            {
                // Log Error
                logger.LogError(new EventId((int)HttpStatusCode.InternalServerError, $"{methodName} > {nameof(CreatePostResponse)}"), ex, "Exception");
                return CreateInternalServerErrorResponse($"{methodName} > {nameof(CreatePostResponse)} > {ex.Message}");
            }
        }

        /// <summary>
        /// Creates the response for PUT methods.
        /// </summary>
        /// <typeparam name="TEntityPayload">Payload entity.</typeparam>
        /// <typeparam name="TEntity">Model entity.</typeparam>
        /// <param name="compileErrors">Delegate to compile errors</param>
        /// <param name="service">Storage service for TEntity.</param>
        /// <param name="id">ID for item to update.</param>
        /// <param name="payload">Payload.</param>
        /// <param name="path">Request path.</param>
        /// <param name="autoMapper">Mapper.</param>
        /// <param name="parameterErrorList">List of parameter validation error messages.</param>
        /// <param name="logger">NGSA Logger.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="methodName">Caller member name to improve logging.</param>
        /// <returns>A task with the appropriate response.</returns>
        public static async Task<ActionResult> CreatePutResponse<TEntityPayload, TEntity>(Func<TEntityPayload, IBaseService<TEntity>, TEntity, Task<IEnumerable<string>>> compileErrors, IBaseService<TEntity> service, string id, TEntityPayload payload, string path, IMapper autoMapper, IEnumerable<string> parameterErrorList, ILogger logger, CancellationToken cancellationToken, [CallerMemberName] string methodName = null)
        {
            try
            {
                // Get existing object to be updated.
                var existingItemResp = await CreateGetByIdResponse<TEntity>(service.Get, id, path, parameterErrorList, logger);

                if (existingItemResp.GetType() != typeof(OkObjectResult))
                {
                    return existingItemResp;
                }

                var existingItem = (TEntity)((ObjectResult)existingItemResp).Value;

                // Map LoadTestConfigPayload to existing LoadTestConfig.
                autoMapper.Map<TEntityPayload, TEntity>(payload, existingItem);

                // Validate and compile errors before post
                var errorList = await compileErrors(payload, service, existingItem);

                // Get POST response
                ActionResult updatedItemResponse = await CreatePostResponse(service.Post, existingItem, path, errorList, logger, cancellationToken);

                // Internal server error response due to no returned value from storage create
                if (updatedItemResponse.GetType() == typeof(CreatedResult))
                {
                    return new NoContentResult();
                }

                return updatedItemResponse;
            }
            catch (Exception ex)
            {
                // Log Error
                logger.LogError(new EventId((int)HttpStatusCode.InternalServerError, $"{methodName} > {nameof(CreatePutResponse)}"), ex, "Exception");
                return CreateInternalServerErrorResponse($"{methodName} > {nameof(CreatePutResponse)} > {ex.Message}");
            }
        }

        public static async Task<ActionResult> CreateDeleteResponse<TEntity>(Func<string, IBaseService<TEntity>, Task<ActionResult>> preDeleteChecks, IBaseService<TEntity> service, string id, string notFound, string unableToDelete, HttpRequest request, ILogger logger)
        where TEntity : class
        {
            try
            {
                // Validate id before deleting
                var preChecksResponse = await ValidateEntityId<TEntity>(id, logger, request);

                if (preChecksResponse != null)
                {
                    return preChecksResponse;
                }

                // Check additional controller-specific conditions, if any, before deleting
                if (preDeleteChecks != null)
                {
                    preChecksResponse = await preDeleteChecks(id, service);

                    if (preChecksResponse != null)
                    {
                        return preChecksResponse;
                    }
                }

                // Delete
                HttpStatusCode delStatusCode = await service.Delete(id);

                // Handle reponse code
                return await HandleDeleteStatusCode(delStatusCode, notFound, unableToDelete);
            }
            catch (CosmosException ce)
            {
                // Log Error
                logger.LogError(new EventId((int)ce.StatusCode, $" {nameof(CreateDeleteResponse)}"), ce, "CosmosException");

                // Returns most common Exception: 404 NotFound, 429 TooManyReqs
                return CreateInternalServerErrorResponse($"{nameof(CreateDeleteResponse)} > CosmosException > [{ce.StatusCode}] {ce.Message}");
            }
            catch (Exception ex)
            {
                // Log Error
                logger.LogError(new EventId((int)HttpStatusCode.InternalServerError, $"{nameof(CreateDeleteResponse)}"), ex, "Exception");
                return CreateInternalServerErrorResponse($"{nameof(CreateDeleteResponse)} > {ex.Message}");
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
                ContentType = JsonContentTypeApplicationJsonProblem,
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
        public static async Task<JsonResult> CreateResult<TEntity>(TEntity data, HttpStatusCode statusCode, string contentType = "application/json")
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
                return await CreateErrorResult("Requested data not found.", HttpStatusCode.NotFound);
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
                ContentType = JsonContentTypeApplicationJsonProblem,
            };
        }

        private static JsonResult CreateValidationErrorResponse(string type, string path, IEnumerable<string> errorList)
        {
            Dictionary<string, object> data = new ()
            {
                { "title", type },
                { "detail", type },
                { "status", (int)HttpStatusCode.BadRequest },
                { "instance", path },
                { "validationErrors", errorList },
            };

            if (type == SystemConstants.InvalidParameter)
            {
                data["type"] = ValidationError.GetErrorLink(path);
            }

            return new JsonResult(data)
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                ContentType = JsonContentTypeApplicationJsonProblem,
            };
        }

        private static async Task<ActionResult> HandleDeleteStatusCode(HttpStatusCode delStatusCode, string notFound, string unableToDelete)
        {
            return delStatusCode switch
            {
                HttpStatusCode.OK =>
                    await ResultHandler.CreateNoContent(),
                HttpStatusCode.NoContent =>
                    await ResultHandler.CreateNoContent(),
                HttpStatusCode.NotFound =>
                    await ResultHandler.CreateErrorResult(notFound, HttpStatusCode.NotFound),
                _ =>
                    await ResultHandler.CreateErrorResult(unableToDelete, HttpStatusCode.InternalServerError),
            };
        }

        private static async Task<ActionResult> ValidateEntityId<TEntity>(string id, ILogger logger, HttpRequest request)
        where TEntity : class
        {
            var errorlist = ParametersValidator<TEntity>.ValidateEntityId(id);

            if (errorlist.Count > 0)
            {
                logger.LogWarning(new EventId((int)HttpStatusCode.BadRequest, nameof(ValidateEntityId)), SystemConstants.InvalidLoadTestConfigId);
                return await ResultHandler.CreateBadRequestResult(errorlist, RequestLogger.GetPathAndQuerystring(request));
            }

            return null;
        }
    }
}
