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
        /// Reusable try-catch block to encapsulate HTTP methods whiel handling cosmos exceptions and other exceptions.
        /// </summary>
        /// <param name="logger">NGSA Logger.</param>
        /// <param name="methodName">String containing caller member name to improve logging.</param>
        /// <param name="taskToExecute">Task to be executed in try block</param>
        /// <returns>A task with the appropriate response.</returns>
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
                logger.LogError(new EventId((int)HttpStatusCode.InternalServerError, $"{methodName}"), ex, "Exception");
                return CreateInternalServerErrorResponse($"{methodName} > {ex.Message}");
            }
        }

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
            return await TryCatchException(logger, $"{methodName} > {nameof(CreateGetResponse)}", async () =>
            {
                var result = await getResult();

                // No content response
                if (!(result as IEnumerable<object>).Any())
                {
                    return new NoContentResult();
                }

                // OK response
                return new OkObjectResult(result);
            });
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
            string updatedMethodName = $"{methodName} > {nameof(CreateGetByIdResponse)}";

            return await TryCatchException(logger, updatedMethodName, async () =>
            {
                // Bad request response due to invalid ID
                var invalidIdResponse = ValidateEntityId<TEntity>(id, logger, request);

                if (invalidIdResponse != null)
                {
                    return invalidIdResponse;
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
            string updatedMethodName = $"{methodName} > {nameof(CreatePostResponse)}";

            return await TryCatchException(logger, updatedMethodName, async () =>
            {
                // Bad request response due to invalid payload
                if (errorList != null && errorList.Any())
                {
                    logger.LogWarning(new EventId((int)HttpStatusCode.BadRequest, updatedMethodName), $"{SystemConstants.BadRequest}: {SystemConstants.InvalidPayload}");
                    return CreateValidationErrorResponse(SystemConstants.InvalidPayload, path, errorList);
                }

                var result = await postResult(payload, cancellationToken);

                // Internal server error response due to no returned value from storage create
                if (result == null)
                {
                    logger.LogError(new EventId((int)HttpStatusCode.InternalServerError, updatedMethodName), null, SystemConstants.UpsertError);
                    return CreateInternalServerErrorResponse($"{updatedMethodName} > {SystemConstants.UpsertError}");
                }

                // Created response
                return new CreatedResult(path, result);
            });
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

        /// <summary>
        /// Creates the response for DELETE methods.
        /// </summary>
        /// <typeparam name="TEntity">Model entity.</typeparam>
        /// <param name="preDeleteChecks">Optional custom pre-deletion checks for each model entity</param>
        /// <param name="service">Storage service for TEntity.</param>
        /// <param name="id">ID for item to update.</param>
        /// <param name="unableToDelete">tring constant for "Unable to delete" response</param>
        /// <param name="request">Request Object</param>
        /// <param name="logger">NGSA Logger.</param>
        /// <param name="methodName">String containing caller member name to improve logging.</param>
        /// <returns>A task with the appropriate response.</returns>
        public static async Task<ActionResult> CreateDeleteResponse<TEntity>(Func<string, IBaseService<TEntity>, Task<ActionResult>> preDeleteChecks, IBaseService<TEntity> service, string id, string unableToDelete, HttpRequest request, ILogger logger, [CallerMemberName] string methodName = null)
        where TEntity : class
        {
            return await TryCatchException(logger, $"{methodName} > {nameof(CreateDeleteResponse)}", async () =>
            {
                // Bad request response due to invalid ID
                var invalidIdResponse = ValidateEntityId<TEntity>(id, logger, request);

                if (invalidIdResponse != null)
                {
                    return invalidIdResponse;
                }

                // Check additional controller-specific conditions, if any, before deleting
                var failedPreCheckResponse = await preDeleteChecks(id, service);

                if (failedPreCheckResponse != null)
                {
                    return failedPreCheckResponse;
                }

                // Delete
                HttpStatusCode delStatusCode = await service.Delete(id);

                // Handle reponse code
                return HandleDeleteStatusCode(delStatusCode);
            });
        }

        /// <summary>
        /// Creates response for conflict error.
        /// </summary>
        /// <param name="message">The Message.</param>
        /// <returns>JsonResult.</returns>
        public static JsonResult CreateConflictErrorResponse(string message)
        {
            return new JsonResult(new ErrorResult { Error = HttpStatusCode.Conflict, Message = $"Conflict: {message}" })
            {
                StatusCode = (int)HttpStatusCode.Conflict,
                ContentType = JsonContentTypeApplicationJsonProblem,
            };
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

        /// <summary>
        /// Create  JSONResult for Validation errors
        /// </summary>
        /// <param name="type">String describing error type</param>
        /// <param name="path">Query path</param>
        /// <param name="errorList">List of errors.</param>
        /// <returns>JsonResult.</returns>
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

        /// <summary>
        /// Parameter validation.
        /// </summary>
        /// <typeparam name="TEntity">Model Entity</typeparam>
        /// <param name="id">Entity id</param>
        /// <param name="logger">NGSA logger.</param>
        /// <param name="request">HTTP Request</param>
        /// <returns>Returns an ActionResult if ValidateEntityId returns any errors, otherwise null.</returns>
        private static ActionResult ValidateEntityId<TEntity>(string id, ILogger logger, HttpRequest request)
        where TEntity : class
        {
            // TODO: Use by all id validators
            var errorlist = ParametersValidator<TEntity>.ValidateEntityId(id);

            if (errorlist.Count > 0)
            {
                logger.LogWarning(new EventId((int)HttpStatusCode.BadRequest, nameof(ValidateEntityId)), SystemConstants.InvalidLoadTestConfigId);
                return ResultHandler.CreateValidationErrorResponse(SystemConstants.InvalidParameter, RequestLogger.GetPathAndQuerystring(request), errorlist);
            }

            return null;
        }

        /// <summary>
        /// Returns appropriate ActionResult based on delete status code.
        /// </summary>
        /// <param name="delStatusCode">Deletion status code</param>
        /// <returns>ActionResult.</returns>
        private static ActionResult HandleDeleteStatusCode(HttpStatusCode delStatusCode)
        {
            return delStatusCode switch
            {
                HttpStatusCode.OK => new NoContentResult(),
                HttpStatusCode.NoContent => new NoContentResult(),
                HttpStatusCode.NotFound => new NotFoundResult(),
                _ => ResultHandler.CreateInternalServerErrorResponse($"{SystemConstants.UnhandledCosmosStatusCode} [StatusCode: {delStatusCode}]"),
            };
        }
    }
}
