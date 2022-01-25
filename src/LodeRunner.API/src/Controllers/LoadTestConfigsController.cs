﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using LodeRunner.API.Middleware;
using LodeRunner.Core.Models;
using LodeRunner.Data.Interfaces;
using LodeRunner.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Swashbuckle.AspNetCore.Annotations;

namespace LodeRunner.API.Controllers
{
    /// <summary>
    /// Handle all of the /api/loadtestconfigs requests.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Create, read, update LoadTest Configurations")]
    public class LoadTestConfigsController : Controller
    {
        private static readonly NgsaLog Logger = new ()
        {
            Name = typeof(LoadTestConfigsController).FullName,
            ErrorMessage = "LoadTestConfigsControllerException",
            NotFoundError = "Load Test Configs Not Found",
        };

        private readonly IMapper autoMapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadTestConfigsController"/> class.
        /// </summary>
        /// <param name="mapper">The mapper.</param>
        public LoadTestConfigsController(IMapper mapper)
        {
            this.autoMapper = mapper;
        }

        /// <summary>
        /// Returns a JSON array of LoadTestConfig objects.
        /// </summary>
        /// <param name="loadTestConfigService">The loadTestConfig service for LRAPI</param>
        /// <param name="cancellationTokenSource">The cancellation Token Source.</param>
        /// <returns>IActionResult.</returns>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, "Array of `LoadTestConfig` documents.", typeof(LoadTestConfig[]), "application/json")]
        [SwaggerResponse((int)HttpStatusCode.NoContent, "`Data not found.`", null, "text/plain")]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, SystemConstants.TerminationDescription)]
        [SwaggerOperation(
            Summary = "Gets a JSON array of LoadTestConfig objects",
            Description = "Returns an array of `LoadTestConfig` documents",
            OperationId = "GetLoadTestConfigs")]
        public async Task<ActionResult<IEnumerable<LoadTestConfig>>> GetLoadTestConfigs([FromServices] ILoadTestConfigService loadTestConfigService, [FromServices] CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
            {
                return await ResultHandler.CreateCancellationInProgressResult();
            }

            List<LoadTestConfig> loadTestConfigs = (List<LoadTestConfig>)await loadTestConfigService.GetAll();
            if (loadTestConfigs.Count == 0)
            {
                return await ResultHandler.CreateNoContent();
            }

            return await ResultHandler.CreateResult(loadTestConfigs, HttpStatusCode.OK);
        }

        /// <summary>
        /// Creates the load test configuration.
        /// </summary>
        /// <param name="loadTestConfigPayload">The load test configuration.</param>
        /// <param name="loadTestConfigService">load Test Config Service.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <returns>IActionResult.</returns>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.Created, "`LoadTestConfig` was created.", typeof(LoadTestConfig), "application/json")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, SystemConstants.InvalidPayloadData)]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, SystemConstants.TerminationDescription)]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, SystemConstants.UnableToCreateLoadTestConfig)]
        [SwaggerOperation(
            Summary = "Creates a new LoadTestConfig item",
            Description = "Requires Load Test Config payload",
            OperationId = "CreateLoadTestConfig")]
        public async Task<ActionResult> CreateLoadTestConfig([FromBody, SwaggerRequestBody("The load test config payload", Required = true)] LoadTestConfigPayload loadTestConfigPayload, [FromServices] ILoadTestConfigService loadTestConfigService, [FromServices] CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
            {
                return await ResultHandler.CreateCancellationInProgressResult();
            }

            // NOTE: the Mapping configuration will create a new loadTestConfig but will ignore the Id since the property has a getter and setter.
            var newloadTestConfig = this.autoMapper.Map<LoadTestConfigPayload, LoadTestConfig>(loadTestConfigPayload);

            if (newloadTestConfig.Validate(out string errorMessage, loadTestConfigPayload.PropertiesChanged))
            {
                var insertedLoadTestConfigResponse = await loadTestConfigService.Post(newloadTestConfig, cancellationTokenSource.Token);

                if (insertedLoadTestConfigResponse.Model != null && insertedLoadTestConfigResponse.StatusCode == HttpStatusCode.OK)
                {
                    return await ResultHandler.CreateResult(insertedLoadTestConfigResponse.Model, HttpStatusCode.OK);
                }
                else if (insertedLoadTestConfigResponse.StatusCode == HttpStatusCode.BadRequest)
                {
                    return await ResultHandler.CreateBadRequestResult(insertedLoadTestConfigResponse.Errors, RequestLogger.GetPathAndQuerystring(this.Request));
                }
                else
                {
                    return await ResultHandler.CreateErrorResult(SystemConstants.UnableToCreateLoadTestConfig, HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                return await ResultHandler.CreateErrorResult($"Invalid payload data. {errorMessage}", HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Deletes the load test configuration.
        /// </summary>
        /// <param name="loadTestConfigId">The Load Test Config id to delete</param>
        /// <param name="loadTestConfigService">The load Test Config Service.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <returns>IActionResult.</returns>
        [HttpDelete("{loadTestConfigId}")]
        [SwaggerResponse((int)HttpStatusCode.OK, SystemConstants.DeletedLoadTestConfig)]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, SystemConstants.InvalidLoadTestConfigId, typeof(Middleware.Validation.ValidationError), "application/problem+json")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, SystemConstants.NotFoundLoadTestConfig)]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, SystemConstants.TerminationDescription)]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, SystemConstants.UnableToDeleteLoadTestConfig)]
        [SwaggerOperation(
            Summary = "Deletes a LoadTestConfig item",
            Description = "Requires Load Test Config id",
            OperationId = "DeleteLoadTestConfig")]
        public async Task<ActionResult> DeleteLoadTestConfig([FromRoute, SwaggerRequestBody("The Load Test Config id to delete", Required = true)] string loadTestConfigId, [FromServices] ILoadTestConfigService loadTestConfigService, [FromServices] CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
            {
                return await ResultHandler.CreateCancellationInProgressResult();
            }

            List<Middleware.Validation.ValidationError> errorlist = ParametersValidator<LoadTestConfig>.ValidateEntityId(loadTestConfigId);

            if (errorlist.Count > 0)
            {
                await Logger.LogWarning(nameof(DeleteLoadTestConfig), SystemConstants.InvalidLoadTestConfigId, NgsaLog.LogEvent400, this.HttpContext);

                return await ResultHandler.CreateBadRequestResult(errorlist, RequestLogger.GetPathAndQuerystring(this.Request));
            }

            var deleteTaskResult = await loadTestConfigService.Delete(loadTestConfigId);

            return deleteTaskResult switch
            {
                HttpStatusCode.OK => await ResultHandler.CreateResult(SystemConstants.DeletedLoadTestConfig, HttpStatusCode.OK),
                HttpStatusCode.NotFound => await ResultHandler.CreateErrorResult(SystemConstants.NotFoundLoadTestConfig, HttpStatusCode.NotFound),
                _ => await ResultHandler.CreateErrorResult(SystemConstants.UnableToDeleteLoadTestConfig, HttpStatusCode.InternalServerError),
            };
        }

        /// <summary>
        /// Updates a load test configuration.
        /// The payload doesn't have to be a full LoadTestConfig item.
        /// Partial payload with only changed fields are accepted too.
        /// </summary>
        /// <example>
        /// Payload Example 1: {"duration":222}
        ///         Example 2: {
        ///                      "baseURL": "lode-url.org",
        ///                      "runLoop": true,
        ///                      "duration": 333,
        ///                      "timeout": 303,
        ///                      "files":[ "/path/to/gem.json" ]
        ///                    }
        /// </example>
        /// <param name="loadTestConfigId">The load test config id.</param>
        /// <param name="loadTestConfigPayload">The load test config payload.</param>
        /// <param name="loadTestConfigService">Load test config Service.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <returns>IActionResult.</returns>
        [HttpPut("{loadTestConfigId}")]
        [SwaggerResponse((int)HttpStatusCode.NoContent, "`LoadTestConfig` was updated.", null, "application/json")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, SystemConstants.UnableToUpdateLoadTestConfig)]
        [SwaggerResponse((int)HttpStatusCode.NotFound, SystemConstants.UnableToGetLoadTestConfig)]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, SystemConstants.InvalidPayloadData)]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, SystemConstants.TerminationDescription)]
        [SwaggerOperation(
            Summary = "Updates an existing LoadTestConfig item",
            Description = "Requires load test config payload (partial or full) and ID",
            OperationId = "UpdateLoadTestConfig")]
        public async Task<ActionResult> UpdateLoadTestConfig([FromRoute] string loadTestConfigId, [FromBody, SwaggerRequestBody("The load test config payload", Required = true)] LoadTestConfigPayload loadTestConfigPayload, [FromServices] ILoadTestConfigService loadTestConfigService, [FromServices] CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
            {
                return await ResultHandler.CreateCancellationInProgressResult();
            }

            // First get the object for verification
            LoadTestConfig existingLoadTestConfig;
            try
            {
                existingLoadTestConfig = await loadTestConfigService.Get(loadTestConfigId);
            }
            catch (CosmosException cex)
            {
                // Returns most common Exception: 404 NotFound, 429 TooManyReqs
                return await ResultHandler.CreateErrorResult(cex.Message, cex.StatusCode);
            }
            catch (Exception ex)
            {
                return await ResultHandler.CreateErrorResult($"Unknown server exception: {ex.Message}", HttpStatusCode.InternalServerError);
            }

            // If we get null object without exception, its 404 as well
            if (existingLoadTestConfig == null)
            {
                // We don't have the item with specified ID, throw error
                return await ResultHandler.CreateErrorResult(SystemConstants.UnableToGetLoadTestConfig, HttpStatusCode.NotFound);
            }

            // Map LoadTestConfigPayload to existing LoadTestConfig.
            this.autoMapper.Map<LoadTestConfigPayload, LoadTestConfig>(loadTestConfigPayload, existingLoadTestConfig);

            // Validate the mapped loadTestConfig
            if (existingLoadTestConfig.Validate(out string errorMessage))
            {
                var insertedLoadTestConfigResponse = await loadTestConfigService.Post(existingLoadTestConfig, cancellationTokenSource.Token);

                if (insertedLoadTestConfigResponse.Model != null && insertedLoadTestConfigResponse.StatusCode == HttpStatusCode.OK)
                {
                    return await ResultHandler.CreateResult(insertedLoadTestConfigResponse.Model, HttpStatusCode.OK);
                }
                else if (insertedLoadTestConfigResponse.StatusCode == HttpStatusCode.BadRequest)
                {
                    return await ResultHandler.CreateBadRequestResult(insertedLoadTestConfigResponse.Errors, RequestLogger.GetPathAndQuerystring(this.Request));
                }
                else
                {
                    return await ResultHandler.CreateErrorResult(SystemConstants.UnableToCreateLoadTestConfig, HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                return await ResultHandler.CreateErrorResult($"{SystemConstants.InvalidPayloadData} {errorMessage}", HttpStatusCode.BadRequest);
            }
        }
    }
}
