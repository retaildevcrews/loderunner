// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.Core;
using LodeRunner.Core.Models;
using LodeRunner.Core.Responses;

namespace LodeRunner.Data.Interfaces
{
    /// <summary>
    /// TestRun Interface.
    /// </summary>
    public interface ITestRunService
    {
        /// <summary>
        /// Posts the specified test run.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The Inserted TestRun entity.</returns>
        Task<ApiResponse<TestRun>> Post(TestRun testRun, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes the specified identifier.
        /// </summary>
        /// <param name="id">The identifier to delete.</param>
        /// <returns>The delete request status code.</returns>
        Task<HttpStatusCode> Delete(string id);

        /// <summary>
        /// Gets all available TestRuns for the given client id.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <returns>List of TestRuns to run on client.</returns>
        Task<IEnumerable<TestRun>> GetAvailableTestRunsByClientIdAsync(string clientId);
    }
}
