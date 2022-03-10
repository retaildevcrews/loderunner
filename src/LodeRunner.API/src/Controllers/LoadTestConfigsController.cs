// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using LodeRunner.API.Middleware;
using LodeRunner.Core.Models;
using LodeRunner.Core.Models.Validators;
using LodeRunner.Data.Interfaces;
using LodeRunner.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger logger;
        private readonly IMapper autoMapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadTestConfigsController"/> class.
        /// </summary>
        /// <param name="mapper">The mapper.</param>
        /// <param name="logger">The logger.</param>
        public LoadTestConfigsController(IMapper mapper, ILogger<LoadTestConfigsController> logger)
        {
            this.autoMapper = mapper;
            this.logger = logger;
        }

        /// <summary>
        /// Returns a JSON array of LoadTestConfig objects.
        /// </summary>
        /// <param name="loadTestConfigService">The loadTestConfig service for LRAPI</param>
        /// <param name="cancellationTokenSource">The cancellation Token Source.</param>
        /// <returns>IActionResult.</returns>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, SystemConstants.AllLoadTestConfigsFound, typeof(LoadTestConfig[]), "application/json")]
        [SwaggerResponse((int)HttpStatusCode.NoContent, SystemConstants.NoLoadTestConfigsFound, null, "text/plain")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, SystemConstants.UnableToGetLoadTestConfigs, typeof(ErrorResult), "application/problem+json")]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, SystemConstants.TerminationDescription)]
        [SwaggerOperation(
            Summary = "Gets a JSON array of LoadTestConfig objects",
            Description = "Returns an array of `LoadTestConfig` documents",
            OperationId = "GetLoadTestConfigs")]
        public async Task<ActionResult<IEnumerable<LoadTestConfig>>> GetLoadTestConfigs([FromServices] ILoadTestConfigService loadTestConfigService, [FromServices] CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
            {
                return ResultHandler.CreateServiceUnavailableResponse();
            }

            return await ResultHandler.CreateGetResponse(loadTestConfigService.GetAll, logger);
        }

        /// <summary>
        /// Returns a single JSON LoadTestConfig by Parameter, LoadTestConfigId.
        /// </summary>
        /// <param name="loadTestConfigId">LoadTestConfigId.</param>
        /// <param name="loadTestConfigService">The LoadTestConfigService.</param>
        /// <param name="cancellationTokenSource">The cancellation Token Source.</param>
        /// <returns>IActionResult.</returns>
        [HttpGet("{loadTestConfigId}")]
        [SwaggerResponse((int)HttpStatusCode.OK, SystemConstants.LoadTestConfigItemFound, typeof(LoadTestConfig), "application/json")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, SystemConstants.InvalidLoadTestConfigId, typeof(Dictionary<string, object>), "application/problem+json")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, SystemConstants.LoadTestConfigItemNotFound, null, "text/plain")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, SystemConstants.UnableToGetLoadTestConfigItem, typeof(ErrorResult), "application/problem+json")]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, SystemConstants.TerminationDescription)]
        [SwaggerOperation(
            Summary = "Gets a single JSON LoadTestConfig by Parameter, loadTestConfigId.",
            Description = "Returns a single `LoadTestConfig` document by loadTestConfigId",
            OperationId = "GetLoadTestConfigByLoadTestConfigId")]
        public async Task<ActionResult<LoadTestConfig>> GetLoadTestConfigById([FromRoute] string loadTestConfigId, [FromServices] ILoadTestConfigService loadTestConfigService, [FromServices] CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
            {
                return ResultHandler.CreateServiceUnavailableResponse();
            }

            return await ResultHandler.CreateGetByIdResponse(loadTestConfigService.Get, loadTestConfigId, this.Request, logger);
        }

        /// <summary>
        /// Creates the load test configuration.
        /// </summary>
        /// <param name="loadTestConfigPayload">The load test configuration.</param>
        /// <param name="loadTestConfigService">load Test Config Service.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <returns>IActionResult.</returns>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.Created, SystemConstants.CreatedLoadTestConfig, typeof(LoadTestConfig), "application/json")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, SystemConstants.InvalidPayload, typeof(Dictionary<string, object>), "application/problem+json")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, SystemConstants.UnableToCreateLoadTestConfig, typeof(ErrorResult), "application/problem+json")]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, SystemConstants.TerminationDescription)]
        [SwaggerOperation(
            Summary = "Creates a new LoadTestConfig item",
            Description = "Requires Load Test Config payload",
            OperationId = "CreateLoadTestConfig")]
        public async Task<ActionResult> CreateLoadTestConfig([FromBody, SwaggerRequestBody("The load test config payload", Required = true)] LoadTestConfigPayload loadTestConfigPayload, [FromServices] LoadTestConfigService loadTestConfigService, [FromServices] CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
            {
                return ResultHandler.CreateServiceUnavailableResponse();
            }

            // NOTE: the Mapping configuration will create a new loadTestConfig but will ignore the Id since the property has a getter and setter.
            var newLoadTestConfig = this.autoMapper.Map<LoadTestConfigPayload, LoadTestConfig>(loadTestConfigPayload);

            return await ResultHandler.CreatePostResponse(this.CompileErrorList, loadTestConfigService, newLoadTestConfig, this.Request, logger, cancellationTokenSource.Token);
        }

        /// <summary>
        /// Updates a load test configuration.
        /// The payload doesn't have to be a full LoadTestConfig item.
        /// Partial payload with only changed fields are accepted too.
        /// </summary>
        /// <param name="loadTestConfigId">The load test config id.</param>
        /// <param name="loadTestConfigPayload">The load test config payload.</param>
        /// <param name="loadTestConfigService">Load test config Service.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <returns>IActionResult.</returns>
        [HttpPut("{loadTestConfigId}")]
        [SwaggerResponse((int)HttpStatusCode.NoContent, SystemConstants.UpdatedLoadTestConfig, null, "text/plain")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, SystemConstants.UnableToUpdateLoadTestConfigNotFound, null, "text/plain")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, SystemConstants.InvalidPayload, typeof(Dictionary<string, object>), "application/problem+json")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, SystemConstants.UnableToUpdateLoadTestConfig, typeof(ErrorResult), "application/problem+json")]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, SystemConstants.TerminationDescription)]
        [SwaggerOperation(
            Summary = "Updates an existing LoadTestConfig item",
            Description = "Requires load test config payload (partial or full) and ID",
            OperationId = "UpdateLoadTestConfig")]
        public async Task<ActionResult> UpdateLoadTestConfig([FromRoute] string loadTestConfigId, [FromBody, SwaggerRequestBody("The load test config payload", Required = true)] LoadTestConfigPayload loadTestConfigPayload, [FromServices] ILoadTestConfigService loadTestConfigService, [FromServices] CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
            {
                return ResultHandler.CreateServiceUnavailableResponse();
            }

            return await ResultHandler.CreatePutResponse(this.CompileErrorList, loadTestConfigService, this.Request, loadTestConfigId, loadTestConfigPayload, this.autoMapper, logger, cancellationTokenSource.Token);
        }

        /// <summary>
        /// Deletes the load test configuration.
        /// </summary>
        /// <param name="loadTestConfigId">The Load Test Config id to delete</param>
        /// <param name="loadTestConfigService">The load Test Config Service.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <returns>IActionResult.</returns>
        [HttpDelete("{loadTestConfigId}")]
        [SwaggerResponse((int)HttpStatusCode.NoContent, SystemConstants.DeletedLoadTestConfig, null, "text/plain")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, SystemConstants.InvalidLoadTestConfigId, typeof(Dictionary<string, object>), "application/problem+json")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, SystemConstants.UnableToDeleteLoadTestConfigNotFound, null, "text/plain")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, SystemConstants.UnableToDeleteLoadTestConfig, typeof(ErrorResult), "application/problem+json")]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, SystemConstants.TerminationDescription)]
        [SwaggerOperation(
            Summary = "Deletes a LoadTestConfig item",
            Description = "Requires Load Test Config id",
            OperationId = "DeleteLoadTestConfig")]
        public async Task<ActionResult> DeleteLoadTestConfig([FromRoute, SwaggerRequestBody("The Load Test Config id to delete", Required = true)] string loadTestConfigId, [FromServices] ILoadTestConfigService loadTestConfigService, [FromServices] CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
            {
                return ResultHandler.CreateServiceUnavailableResponse();
            }

            return await ResultHandler.CreateDeleteResponse<LoadTestConfig>(null, loadTestConfigService, loadTestConfigId, this.Request, logger);
        }

        /// <summary>
        /// Compile and return entity validation errors.
        /// </summary>
        /// <param name="service">Storage service for LoadTestConfig.</param>
        /// <param name="newLoadTestConfig">LoadTestConfig</param>
        /// <returns>List of error strings.</returns>
        private IEnumerable<string> CompileErrorList(IBaseService<LoadTestConfig> service, LoadTestConfig newLoadTestConfig)
        {
            var payloadErrorList = service.Validator.ValidateEntity(newLoadTestConfig);
            var flagErrorList = newLoadTestConfig.FlagValidator();
            var errorList = flagErrorList.Concat<string>(payloadErrorList);
            return errorList;
        }
    }
}
