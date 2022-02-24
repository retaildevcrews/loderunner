// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net;
using System.Threading.Tasks;
using LodeRunner.Core.Models;
using LodeRunner.Core.Responses;
using LodeRunner.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace LodeRunner.API.Extensions
{
    public static class TestRunsServiceExtensions
    {
        /// <summary>
        /// Gets TestRunId.
        /// </summary>
        /// <param name="testRunService">The test run service.</param>
        /// <param name="testRunId">The testRunId.</param>
        /// <returns>The Task</returns>
        public static async Task<ApiResponse<TestRun>> GetTestRun(this TestRunService testRunService, string testRunId)
        {
            ApiResponse<TestRun> result = new ();

            // First get the object for verification
            TestRun existingTestRun = null;
            try
            {
                existingTestRun = await testRunService.Get(testRunId);

                result.Model = existingTestRun;
                result.StatusCode = HttpStatusCode.OK;
            }
            catch (CosmosException cex)
            {
                // Returns most common Exception: 404 NotFound, 429 TooManyReqs
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.Errors = cex.Message;
            }
            catch (Exception ex)
            {
                result.StatusCode = HttpStatusCode.InternalServerError;
                result.Errors = $"Unknown server exception: {ex.Message}";
            }

            // If we get null object without exception, its 404 as well
            if (existingTestRun == null)
            {
                // We don't have the item with specified ID, throw error
                result.StatusCode = HttpStatusCode.NotFound;
                result.Errors = SystemConstants.TestRunItemNotFound;
            }

            return result;
        }

        /// <summary>
        /// Gets the test run by identifier.
        /// </summary>
        /// <param name="testRunService">The test run service.</param>
        /// <param name="testRunId">The test run identifier.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>The Task.</returns>
        public static async Task<TestRun> GetTestRunById(this TestRunService testRunService, string testRunId, ILogger logger)
        {
            TestRun result = null;
            try
            {
                // Get Test Run from Cosmos

                TestRun testRun = await testRunService.Get(testRunId);

                if (testRun != null)
                {
                    result = testRun;
                }
            }
            catch (CosmosException ce)
            {
                // log and return Cosmos status code
                if (ce.StatusCode == HttpStatusCode.NotFound)
                {
                    logger.LogWarning(new EventId((int)ce.StatusCode, nameof(GetTestRunById)), $"Test Runs {SystemConstants.NotFoundError}");
                }
                else
                {
                    throw new Exception($"{nameof(GetTestRunById)}: {ce.Message}", ce);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{nameof(GetTestRunById)}: {ex.Message}", ex);
            }

            return result;
        }
    }
}
