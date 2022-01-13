// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LodeRunner.API.Middleware;
using LodeRunner.API.Models;
using LodeRunner.Core.Models;
using LodeRunner.Core.Responses;
using LodeRunner.Services;
using Microsoft.Azure.Cosmos;

namespace LodeRunner.API.Extensions
{
    public static class TestRunsServiceExtensions
    {
        /// <summary>
        /// Gets TestRunId.
        /// </summary>
        /// <param name="testRunService">The client status service.</param>
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
                result.Errors = SystemConstants.UnableToGetTestRun;
            }

            return result;
        }
    }
}
