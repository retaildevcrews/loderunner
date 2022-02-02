// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using LodeRunner.API.Extensions;
using LodeRunner.API.Middleware;
using LodeRunner.Core.Models;
using LodeRunner.Core.Responses;
using LodeRunner.Data.Interfaces;
using LodeRunner.Services;
using Microsoft.AspNetCore.Mvc;
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
        private static readonly NgsaLog Logger = new ()
        {
            Name = typeof(TestRunsController).FullName,
            ErrorMessage = "TestRunsControllerException",
            NotFoundError = "Test Runs Not Found",
        };

        private readonly IMapper autoMapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestRunsController"/> class.
        /// </summary>
        /// <param name="mapper">The mapper.</param>
        public TestRunsController(IMapper mapper)
        {
            this.autoMapper = mapper;
        }

        /// <summary>
        /// Returns a JSON array of Test Run objects.
        /// </summary>
        /// <param name="testRunService">The testRun service for LRAPI</param>
        /// <param name="cancellationTokenSource">The cancellation Token Source.</param>
        /// <returns>IActionResult.</returns>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, SystemConstants.TestRunsFound, typeof(TestRun[]), "application/json")]
        [SwaggerResponse((int)HttpStatusCode.NoContent, SystemConstants.TestRunsNotFound, null, "text/plain")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, SystemConstants.UnableToGetTestRuns)]
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

            return await ResultHandler.CreateGetResponse(testRunService.GetTestRuns, Logger);
        }

        /// <summary>
        /// Returns a single JSON TestRun by Parameter, testRunId.
        /// </summary>
        /// <param name="testRunId">testRunId.</param>
        /// <param name="testRunService">The TetsRunService.</param>
        /// <param name="cancellationTokenSource">The cancellation Token Source.</param>
        /// <returns>IActionResult.</returns>
        [HttpGet("{testRunId}")]
        [SwaggerResponse((int)HttpStatusCode.OK, "Single `TestRun` document by testRunId.", typeof(TestRun), "application/json")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, SystemConstants.InvalidTestRunId, typeof(Middleware.Validation.ValidationError), "application/problem+json")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, "`Data not found.`", null, "application/json")]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, SystemConstants.TerminationDescription)]
        [SwaggerOperation(
            Summary = "Gets a single JSON TestRun by Parameter, testRunId.",
            Description = "Returns a single `TestRun` document by testRunId",
            OperationId = "GetTestRunByTestRunId")]
        public async Task<ActionResult<TestRun>> GetTestRunById([FromRoute] string testRunId, [FromServices] TestRunService testRunService, [FromServices] CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
            {
                return ResultHandler.CreateServiceUnavailableResponse();
            }

            List<Middleware.Validation.ValidationError> errorlist = ParametersValidator<TestRun>.ValidateEntityId(testRunId);

            if (errorlist.Count > 0)
            {
                await Logger.LogWarning(nameof(GetTestRunById), SystemConstants.InvalidTestRunId, NgsaLog.LogEvent400, this.HttpContext);

                return await ResultHandler.CreateBadRequestResult(errorlist, RequestLogger.GetPathAndQuerystring(this.Request));
            }

            var result = await testRunService.GetTestRunById(testRunId, Logger);

            return await ResultHandler.HandleResult(result, Logger);
        }

        /// <summary>
        /// Creates the test run.
        /// </summary>
        /// <param name="testRunPayload">The test run.</param>
        /// <param name="testRunService">Test Run Service.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <returns>IActionResult.</returns>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.Created, "`TestRun` was created.", typeof(TestRun), "application/json")]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, SystemConstants.InvalidPayloadData)]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, SystemConstants.TerminationDescription)]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, SystemConstants.UnableToCreateTestRun)]
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

            var insertedTestRunResponse = await testRunService.Post(newTestRun, cancellationTokenSource.Token);

            if (insertedTestRunResponse.Model != null && insertedTestRunResponse.StatusCode == HttpStatusCode.OK)
            {
                return await ResultHandler.CreateResult(insertedTestRunResponse.Model, HttpStatusCode.Created);
            }
            else if (insertedTestRunResponse.StatusCode == HttpStatusCode.BadRequest)
            {
                return await ResultHandler.CreateBadRequestResult(insertedTestRunResponse.Errors, RequestLogger.GetPathAndQuerystring(this.Request));
            }
            else
            {
                return await ResultHandler.CreateErrorResult(SystemConstants.UnableToCreateTestRun, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Deletes the TestRun.
        /// </summary>
        /// <param name="testRunId">The Test Run id to delete</param>
        /// <param name="testRunService">The Test Run Service.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <returns>IActionResult.</returns>
        [HttpDelete("{testRunId}")]
        [SwaggerResponse((int)HttpStatusCode.NoContent, SystemConstants.DeletedTestRun)]
        [SwaggerResponse((int)HttpStatusCode.NotFound, SystemConstants.NotFoundTestRun)]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, SystemConstants.TerminationDescription)]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, SystemConstants.UnableToDeleteTestRun)]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, SystemConstants.Unknown)]
        [SwaggerResponse((int)HttpStatusCode.Conflict, SystemConstants.UnableToDeleteRunNotCompleted)]
        [SwaggerOperation(
            Summary = "Deletes a TestRun item",
            Description = "Requires Test Run id",
            OperationId = "DeleteTestRun")]
        public async Task<ActionResult> DeleteTestRun([FromRoute, SwaggerRequestBody("The Test Run id to delete", Required = true)] string testRunId, [FromServices] TestRunService testRunService, [FromServices] CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
            {
                return ResultHandler.CreateServiceUnavailableResponse();
            }

            var existingTestRunResp = await testRunService.GetTestRun(testRunId);
            HttpStatusCode delStatusCode = HttpStatusCode.InternalServerError;

            if (existingTestRunResp.StatusCode == HttpStatusCode.OK)
            {
                if (existingTestRunResp.Model.CompletedTime != null)
                {
                    delStatusCode = await testRunService.Delete(testRunId);
                }
                else
                {
                    delStatusCode = HttpStatusCode.Conflict;
                }
            }

            // Get       Delete    Returns   Message
            // -------------------------------------
            // IntErr    Any       IntErr    Cosmos error
            // NotFound  Any       NotFound  ID not found
            // Ok        NotFound  NotFound  (Redundant) Same as above*
            // Ok        Conflict  Conflict  ID Found but unfinished
            // Ok        Ok        NoContent All good
            // Any       Any       IntErr    But also show GET and DELETE status
            return (existingTestRunResp.StatusCode, delStatusCode) switch
            {
                (HttpStatusCode.InternalServerError, _) =>
                    await ResultHandler.CreateErrorResult(existingTestRunResp.Errors, HttpStatusCode.InternalServerError),
                (HttpStatusCode.NotFound, _) =>
                    await ResultHandler.CreateErrorResult(existingTestRunResp.Errors, HttpStatusCode.NotFound),

                // This case is redundant, but is handy for testing
                // (HttpStatusCode.OK, HttpStatusCode.NotFound) =>
                //     await ResultHandler.CreateErrorResult(SystemConstants.NotFoundTestRun, HttpStatusCode.NotFound),
                (HttpStatusCode.OK, HttpStatusCode.Conflict) =>
                    await ResultHandler.CreateErrorResult($"{SystemConstants.UnableToDeleteRunNotCompleted}. TestRun ID: {testRunId}", HttpStatusCode.Conflict),
                (HttpStatusCode.OK, HttpStatusCode.OK) => await ResultHandler.CreateNoContent(),
                (HttpStatusCode.OK, _) =>
                    await ResultHandler.CreateErrorResult(SystemConstants.UnableToDeleteTestRun, HttpStatusCode.InternalServerError),
                (_, _) => // For all other cases
                    await ResultHandler.CreateErrorResult($"{SystemConstants.Unknown}. TestRun ({testRunId}) GET Status: {existingTestRunResp.StatusCode}, DEL status: {delStatusCode}", HttpStatusCode.InternalServerError),
            };
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
        [SwaggerResponse((int)HttpStatusCode.NoContent, "`TestRun` was updated.", null, "application/json")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, SystemConstants.UnableToUpdateTestRun)]
        [SwaggerResponse((int)HttpStatusCode.ServiceUnavailable, SystemConstants.TerminationDescription)]
        [SwaggerResponse((int)HttpStatusCode.NotFound, SystemConstants.UnableToGetTestRun)]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, SystemConstants.InvalidPayloadData)]
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

            var canGetExistingTestRunResponse = await testRunService.GetTestRun(testRunId);

            switch (canGetExistingTestRunResponse.StatusCode)
            {
                case HttpStatusCode.InternalServerError:
                    await ResultHandler.CreateErrorResult(canGetExistingTestRunResponse.Errors, HttpStatusCode.InternalServerError);
                    break;
                case HttpStatusCode.NotFound:
                    await ResultHandler.CreateErrorResult(canGetExistingTestRunResponse.Errors, HttpStatusCode.NotFound);
                    break;
            }

            // Map TestRunPayload to existing TestRun.
            this.autoMapper.Map<TestRunPayload, TestRun>(testRunPayload, canGetExistingTestRunResponse.Model);

            var insertedTestRunResponse = await testRunService.Post(canGetExistingTestRunResponse.Model, cancellationTokenSource.Token);

            if (insertedTestRunResponse.Model != null && insertedTestRunResponse.StatusCode == HttpStatusCode.OK)
            {
                return await ResultHandler.CreateNoContent();
            }
            else if (insertedTestRunResponse.StatusCode == HttpStatusCode.BadRequest)
            {
                return await ResultHandler.CreateBadRequestResult(insertedTestRunResponse.Errors, RequestLogger.GetPathAndQuerystring(this.Request));
            }
            else
            {
                return await ResultHandler.CreateErrorResult(SystemConstants.UnableToCreateTestRun, HttpStatusCode.InternalServerError);
            }
        }
    }
}
