// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.Core.Models;

namespace LodeRunner.Data.Interfaces
{
    /// <summary>
    /// TestRun Interface.
    /// </summary>
    public interface ITestRunService
    {
        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The corresponding Enity.</returns>
        Task<TestRun> Get(string id);

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns>all items for a given type.</returns>
        Task<IEnumerable<TestRun>> GetAll();

        /// <summary>
        /// Posts the specified test run.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The Inserted TestRun entity.</returns>
        Task<TestRun> Post(TestRun testRun, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes the specified identifier.
        /// </summary>
        /// <param name="id">The identifier to delete.</param>
        /// <returns>The delete request status code.</returns>
        Task<HttpStatusCode> Delete(string id);
    }
}
