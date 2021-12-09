// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using LodeRunner.API.Interfaces;
using LodeRunner.API.Middleware;
using LodeRunner.Core.Models;
using LodeRunner.Data.Interfaces;
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
        [SwaggerResponse((int)HttpStatusCode.OK, "Array of `LoadTestConfig` documents or empty array if not found.", typeof(LoadTestConfig[]), "application/json")]
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
                return new NoContentResult();
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
        [SwaggerResponse((int)HttpStatusCode.OK, "`LoadTestConfig` was created.", typeof(LoadTestConfig), "application/json")]
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
            LoadTestConfig newloadTestConfig = this.autoMapper.Map<LoadTestConfig, LoadTestConfig>(loadTestConfigPayload as LoadTestConfig);

            if (newloadTestConfig.Validate(loadTestConfigPayload.PropertiesChanged, out string errorMessage))
            {
                var insertedLoadTestConfig = await loadTestConfigService.Post(newloadTestConfig, cancellationTokenSource.Token);

                if (insertedLoadTestConfig != null)
                {
                    return await ResultHandler.CreateResult(insertedLoadTestConfig, HttpStatusCode.OK);
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
        [SwaggerResponse((int)HttpStatusCode.NotFound, SystemConstants.NotFoundLoadTestConfig)]
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

            var deleteTaskResult = await loadTestConfigService.Delete(loadTestConfigId);

            switch (deleteTaskResult)
            {
                case HttpStatusCode.OK:
                    return await ResultHandler.CreateResult(SystemConstants.DeletedLoadTestConfig, HttpStatusCode.OK);
                case HttpStatusCode.NotFound:
                    return await ResultHandler.CreateErrorResult(SystemConstants.NotFoundLoadTestConfig, HttpStatusCode.NotFound);
                default:
                    return await ResultHandler.CreateErrorResult(SystemConstants.UnableToDeleteLoadTestConfig, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Updates a load test configuration.
        /// </summary>
        /// <param name="loadTestConfigId">The load test config id.</param>
        /// <param name="loadTestConfigPayload">The load test config payload.</param>
        /// <param name="loadTestConfigService">Load test config Service.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <returns>IActionResult.</returns>
        [HttpPut("{loadTestConfigId}")]
        [SwaggerResponse((int)HttpStatusCode.NoContent, "`LoadTestConfig` was updated.", null, "application/json")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, SystemConstants.UnableToUpdateLoadTestConfig)]
        [SwaggerResponse((int)HttpStatusCode.NotFound, SystemConstants.UnableToGetLoadTestConfig)]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, "Invalid payload data")]
        [SwaggerOperation(
            Summary = "Updates an existing LoadTestConfig item",
            Description = "Requires Load Test Config payload and ID",
            OperationId = "UpdateLoadTestConfig")]
        public async Task<ActionResult> UpdateLoadTestConfig([FromRoute] string loadTestConfigId, [FromBody, SwaggerRequestBody("The load test config payload", Required = true)] LoadTestConfigPayload loadTestConfigPayload, [FromServices] ILoadTestConfigService loadTestConfigService, [FromServices] CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
            {
                return await ResultHandler.CreateCancellationInProgressResult();
            }

            // First get the object for verification
            var existingLoadTestConfig = await loadTestConfigService.Get(loadTestConfigId);

            if (existingLoadTestConfig == null)
            {
                // We don't have the item with specified ID, throw error
                return await ResultHandler.CreateErrorResult(SystemConstants.UnableToGetLoadTestConfig, HttpStatusCode.NotFound);
            }

            // NOTE: the Mapping configuration will create a new loadTestConfig but will ignore the Id since the property has a getter and setter.
            this.autoMapper.Map<LoadTestConfigPayload, LoadTestConfig>(loadTestConfigPayload, existingLoadTestConfig);
            // Replace newLoadTestConfig ID from our URL
            //newLoadTestConfig.Id = loadTestConfigId;

            if (existingLoadTestConfig.Validate(loadTestConfigPayload.PropertiesChanged, out string errorMessage))
            {
                // TODO: Resolve this
                Task<LoadTestConfig> insertedLoadTestConfig = null;
                //await loadTestConfigService.Post(existingLoadTestConfig, cancellationTokenSource.Token);

                if (insertedLoadTestConfig != null)
                {
                    return await ResultHandler.CreateNoContent();
                }
                else
                {
                    return await ResultHandler.CreateErrorResult(SystemConstants.UnableToUpdateLoadTestConfig, HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                return await ResultHandler.CreateErrorResult($"Invalid payload data. {errorMessage}", HttpStatusCode.BadRequest);
            }
        }
    }
}
