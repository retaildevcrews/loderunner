// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net;
using System.Threading;
using AutoMapper;
using LodeRunner.API.Middleware;
using LodeRunner.Core.Models;
using LodeRunner.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace LodeRunner.API.Controllers
{
    /// <summary>
    /// Handle all of the /api/loadtestconfig requests.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Create, read, update LoadTest Configurations")]
    public class LoadTestConfigController : Controller
    {
        private readonly IMapper autoMapper;

        public LoadTestConfigController(IMapper mapper)
        {
            autoMapper = mapper;
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
        public IActionResult CreateLoadTestConfig([FromBody, SwaggerRequestBody("The load test config payload", Required = true)] LoadTestConfigPayload loadTestConfigPayload, [FromServices] ILoadTestConfigService loadTestConfigService, [FromServices] CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
            {
                return ResultHandler.CreateCancellationInProgressResult();
            }

            // NOTE: the Mapping configuration will create a new loadTestConfig but will ignore the Id since the property has a getter and setter.
            LoadTestConfig newloadTestConfig = autoMapper.Map<LoadTestConfig>(loadTestConfigPayload);

            if (newloadTestConfig.Validate(out string errorMessage))
            {
                var insertedLoadTestConfig = loadTestConfigService.Post(newloadTestConfig, cancellationTokenSource.Token).Result;

                if (insertedLoadTestConfig != null)
                {
                    return ResultHandler.CreateResult(insertedLoadTestConfig, HttpStatusCode.OK);
                }
                else
                {
                    return ResultHandler.CreateResult(SystemConstants.UnableToCreateLoadTestConfig, HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                return ResultHandler.CreateResult($"Invalid payload data. {errorMessage}", HttpStatusCode.BadRequest);
            }
        }
    }
}
