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
    /// LoadTestConfig Interface.
    /// </summary>
    public interface ILoadTestConfigService
    {
        /// <summary>
        /// Posts the specified load test configuration.
        /// </summary>
        /// <param name="loadTestConfig">The load test configuration.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The Inserted LoadTestConfig entity.</returns>
        Task<ApiResponse<LoadTestConfig>> Post(LoadTestConfig loadTestConfig, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes the specified identifier.
        /// </summary>
        /// <param name="id">The identifier to delete.</param>
        /// <returns>The delete request status code.</returns>
        Task<HttpStatusCode> Delete(string id);
    }
}
