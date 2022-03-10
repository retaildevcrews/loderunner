// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using LodeRunner.API.Extensions;
using LodeRunner.API.Middleware;
using LodeRunner.Core.Models;
using LodeRunner.Data.Interfaces;
using LodeRunner.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace LodeRunner.API.Controllers
{
    /// <summary>
    /// Handle all of the /api/testruns requests.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Create, read, update, delete Test Runs")]
    public class TestRunsController : Controller
    {
        private readonly ILogger logger;
        private readonly IMapper autoMapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestRunsController"/> class.
        /// </summary>
        /// <param name="mapper">The mapper.</param>
        /// <param name="logger">The logger.</param>
        public TestRunsController(IMapper mapper, ILogger<LoadTestConfigsController> logger)
        {
            this.autoMapper = mapper;
            this.logger = logger;
        }

        /// <summary>
        /// Returns a JSON array of Test Run objects.
        /// </summary>
        /// <param name="testRunService">The testRun service for LRAPI</param>
        /// <param name="cancellationTokenSource">The cancellation Token Source.</param>
        /// <returns>IActionResult.</returns>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, SystemConstants.AllTestRunsFound, typeof(TestRun[]), "application/json")]
        [SwaggerResponse((int)HttpStatusCode.NoContent, SystemConstants.NoTestRunsFound, null, "text/plain")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, SystemConstants.UnableToGetTestRuns, typeof(ErrorResult), "application/problem+json")]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, SystemConstants.TerminationDescription)]
        [SwaggerOperation(
            Summary = "Gets a JSON array of TestRun objects",
            Description = "Returns an array of `TestRun` documents",
            OperationId = "GetTestRuns")]
        public async Task<ActionResult<IEnumerable<TestRun>>> GetTestRuns([FromServices] ITestRunService testRunService, [FromServices] CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
            {
                return ResultHandler.CreateServiceUnavailableResponse();
            }

            return await ResultHandler.CreateGetResponse(testRunService.GetAll, logger);
        }

        /// <summary>
        /// Returns a single JSON TestRun by Parameter, testRunId.
        /// </summary>
        /// <param name="testRunId">testRunId.</param>
        /// <param name="testRunService">The TetsRunService.</param>
        /// <param name="cancellationTokenSource">The cancellation Token Source.</param>
        /// <returns>IActionResult.</returns>
        [HttpGet("{testRunId}")]
        [SwaggerResponse((int)HttpStatusCode.OK, SystemConstants.TestRunItemFound, typeof(TestRun), "application/json")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, SystemConstants.InvalidTestRunId, typeof(Dictionary<string, object>), "application/problem+json")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, SystemConstants.TestRunItemNotFound, null, "application/json")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, SystemConstants.UnableToGetTestRunItem, typeof(ErrorResult), "application/problem+json")]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, SystemConstants.TerminationDescription)]
        [SwaggerOperation(
            Summary = "Gets a single JSON TestRun by Parameter, testRunId.",
            Description = "Returns a single `TestRun` document by testRunId",
            OperationId = "GetTestRunByTestRunId")]
        public async Task<ActionResult<TestRun>> GetTestRunById([FromRoute] string testRunId, [FromServices] ITestRunService testRunService, [FromServices] CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
            {
                return ResultHandler.CreateServiceUnavailableResponse();
            }

            return await ResultHandler.CreateGetByIdResponse(testRunService.Get, testRunId, this.Request, logger);
        }

        /// <summary>
        /// Creates the test run.
        /// </summary>
        /// <param name="testRunPayload">The test run.</param>
        /// <param name="testRunService">Test Run Service.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <returns>IActionResult.</returns>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.Created, SystemConstants.CreatedTestRun, typeof(TestRun), "application/json")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, SystemConstants.InvalidPayload, typeof(Dictionary<string, object>), "application/problem+json")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, SystemConstants.UnableToCreateTestRun, typeof(ErrorResult), "application/problem+json")]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, SystemConstants.TerminationDescription)]
        [SwaggerOperation(
            Summary = "Creates a new TestRun item",
            Description = "Requires Test Run payload. It will not accept overall CompletedTime or ClientResults, which should only be created by the LoadClient internally",
            OperationId = "CreateTestRunConfig")]
        public async Task<ActionResult> CreateTestRunConfig([FromBody, SwaggerRequestBody("The test run config payload", Required = true)] TestRunPayload testRunPayload, [FromServices] TestRunService testRunService, [FromServices] CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
            {
                return ResultHandler.CreateServiceUnavailableResponse();
            }

            // NOTE: the Mapping configuration will create a new testRun but will ignore the Id since the property has a getter and setter.
            var newTestRun = this.autoMapper.Map<TestRunPayload, TestRun>(testRunPayload);

            return await ResultHandler.CreatePostResponse(this.CompileErrorList, testRunService, newTestRun, this.Request, logger, cancellationTokenSource.Token);
        }

        /// <summary>
        /// Updates a test run. Will not update nested properties (i.e. ClientResults).
        /// The payload doesn't have to be a full TestRun item.
        /// </summary>
        /// <example>
        /// Payload Example 1: {"name": "TestRun-Name001"}
        /// </example>
        /// <param name="testRunId">The test run id.</param>
        /// <param name="testRunPayload">The test run payload.</param>
        /// <param name="testRunService">Test Run Service.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <returns>IActionResult.</returns>
        [HttpPut("{testRunId}")]
        [SwaggerResponse((int)HttpStatusCode.NoContent, SystemConstants.UpdatedTestRun, null, "text/plain")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, SystemConstants.UnableToUpdateTestRunNotFound, null, "text/plain")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, SystemConstants.InvalidPayload, typeof(Dictionary<string, object>), "application/problem+json")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, SystemConstants.UnableToUpdateTestRun, typeof(ErrorResult), "application/problem+json")]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, SystemConstants.TerminationDescription)]
        [SwaggerOperation(
            Summary = "Updates an existing TestRun item",
            Description = "Requires test run payload (partial or full) and ID. It will not accept overall CompletedTime or ClientResults, which should only be updated by the LoadClient internally",
            OperationId = "UpdateTestRun")]
        public async Task<ActionResult> UpdateTestRun([FromRoute] string testRunId, [FromBody, SwaggerRequestBody("The test run payload", Required = true)] TestRunPayload testRunPayload, [FromServices] TestRunService testRunService, [FromServices] CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
            {
                return ResultHandler.CreateServiceUnavailableResponse();
            }

            return await ResultHandler.CreatePutResponse(this.CompileErrorList, testRunService, this.Request, testRunId, testRunPayload, this.autoMapper, logger, cancellationTokenSource.Token);
        }

        /// <summary>
        /// Deletes the TestRun.
        /// </summary>
        /// <param name="testRunId">The Test Run id to delete</param>
        /// <param name="testRunService">The Test Run Service.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <returns>IActionResult.</returns>
        [HttpDelete("{testRunId}")]
        [SwaggerResponse((int)HttpStatusCode.NoContent, SystemConstants.DeletedTestRun, null, "text/plain")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, SystemConstants.InvalidTestRunId, typeof(Dictionary<string, object>), "application/problem+json")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, SystemConstants.UnableToDeleteLoadTestConfigNotFound, null, "text/plain")]
        [SwaggerResponse((int)HttpStatusCode.Conflict, SystemConstants.UnableToDeleteTestRunRunning, typeof(ErrorResult), "application/problem+json")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, SystemConstants.UnableToDeleteTestRun, typeof(ErrorResult), "application/problem+json")]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, SystemConstants.TerminationDescription)]
        [SwaggerOperation(
            Summary = "Deletes a TestRun item. Can only delete if TestRun item is Completed or StartTime is scheduled less than a minute from now",
            Description = "Requires Test Run id",
            OperationId = "DeleteTestRun")]
        public async Task<ActionResult> DeleteTestRun([FromRoute, SwaggerRequestBody("The Test Run id to delete", Required = true)] string testRunId, [FromServices] TestRunService testRunService, [FromServices] CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
            {
                return ResultHandler.CreateServiceUnavailableResponse();
            }

            return await ResultHandler.CreateDeleteResponse<TestRun>(RunPreDeletionChecks, testRunService, testRunId, this.Request, logger);
        }

        /// <summary>
        /// Check if conditions are met for testRun deletion.
        /// </summary>
        /// <param name="testRun">TestRun entity.</param>
        /// <returns>Boolean value indicating whether testRun can be deleted.</returns>
        private static bool CanTestRunBeDeleted(TestRun testRun)
        {
            return testRun.CompletedTime != null || (testRun.CompletedTime == null && testRun.StartTime >= DateTime.UtcNow.AddMinutes(1));
        }

        /// <summary>
        /// Compile and return entity validation errors.
        /// </summary>
        /// <param name="service">Storage service for TestRun.</param>
        /// <param name="testRun">TestRun</param>
        /// <returns>List of error strings.</returns>
        private IEnumerable<string> CompileErrorList(IBaseService<TestRun> service, TestRun testRun)
        {
            return service.Validator.ValidateEntity(testRun);
        }

        /// <summary>
        /// Runs GET method to find selected testRun and verify that it can be deleted. Assumes that id formatting, etc. has been validated.
        /// </summary>
        /// <param name="testRunId">The testRun id to delete.</param>
        /// <param name="testRunService">The testRun service.</param>
        /// <returns>Appropriate ActionResult based on error or null if all checks pass.</returns>
        private async Task<ActionResult> RunPreDeletionChecks(string testRunId, IBaseService<TestRun> testRunService)
        {
            // Create and return GET response; assumes that id has been validated.
            return await ResultHandler.TryCatchException(logger, nameof(RunPreDeletionChecks), async () =>
            {
                var result = await testRunService.Get(testRunId);

                // Not found response
                if (result == null)
                {
                    return new NotFoundResult();
                }

                // Check if TestRun can be deleted
                if (!CanTestRunBeDeleted(result))
                {
                    return ResultHandler.CreateConflictErrorResponse($"{SystemConstants.UnableToDeleteTestRunRunning}. TestRun ID: {testRunId}");
                }

                return new EmptyResult();
            });
        }
    }
}
