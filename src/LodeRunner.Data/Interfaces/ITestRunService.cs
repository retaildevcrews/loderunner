// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.Core;
using LodeRunner.Core.Models;

namespace LodeRunner.Data.Interfaces
{
    /// <summary>
    /// TestRun Interface.
    /// </summary>
    public interface ITestRunService : IBaseService<TestRun>
    {
        /// <summary>
        /// Gets all available TestRuns for the given client id.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <returns>List of TestRuns to run on client.</returns>
        Task<IEnumerable<TestRun>> GetNewTestRunsByClientId(string clientId);
    }
}
