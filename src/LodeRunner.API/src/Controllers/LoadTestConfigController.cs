// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using LodeRunner.API.Data.Dtos;
using LodeRunner.API.Interfaces;
using LodeRunner.API.Middleware;
using LodeRunner.API.Models;
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
        private static readonly NgsaLog Logger = new ()
        {
            Name = typeof(LoadTestConfigController).FullName,
            ErrorMessage = "LoadTestConfigControllerControllerException",
            NotFoundError = "LoadTestConfigs Not Found",
        };

        /// <summary>
        /// Creates the load test configuration.
        /// </summary>
        /// <param name="loadTestConfigDTO">The load test configuration dto.</param>
        /// <param name="appCache">The application cache.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <returns>IActionResult.</returns>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.OK, "Load Test Config was created.", typeof(Client[]))]
        [SwaggerOperation(
            Summary = "Creates a new LoadTestConfig item",
            Description = "Requires LoadTest Config payload",
            OperationId = "CreateLoadTestConfig")]
        [Route("create")]
        public IActionResult CreateLoadTestConfig([FromBody, SwaggerRequestBody("The load test config payload", Required = true)] LoadTestConfigDto loadTestConfigDTO, [FromServices] ILRAPICache appCache, [FromServices] CancellationTokenSource cancellationTokenSource)
        {
            // NOTE: We use DTO approach to protect from mass assignment/over posting attacks.
            if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
            {
                return ResultHandler.CreateCancellationInProgressResult();
            }

            if (!ModelState.IsValid)
            {
                return ResultHandler.CreateResult("Invalid Model.", HttpStatusCode.BadRequest);
            }

            if (loadTestConfigDTO.Validate())
            {
                var loadTestConfig = loadTestConfigDTO.DtoToModel();

                appCache.SetLoadTestConfig(loadTestConfig);

                // TODO: Do we need to improve the Result to say something about successfully created ?
                return appCache.HandleCacheResult(loadTestConfig, Logger);
            }
            else
            {
                return ResultHandler.CreateResult("Invalid Data.", HttpStatusCode.BadRequest);
            }
        }
    }
}
