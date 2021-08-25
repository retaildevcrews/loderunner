// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ngsa.LodeRunner.DataAccessLayer.Interfaces
{
    /// <summary>
    /// ClientStatusService Interface.
    /// </summary>
    public interface IClientStatusService
    {
        /// <summary>
        /// Posts the starting.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="lastUpdated">The last updated.</param>
        /// <returns>The Task.</returns>
        Task PostStarting(string message, DateTime? lastUpdated = null);

        /// <summary>
        /// Posts the ready.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="lastUpdated">The last updated.</param>
        /// <returns>The Task.</returns>
        Task PostReady(string message, DateTime? lastUpdated = null);

        /// <summary>
        /// Posts the testing.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="lastUpdated">The last updated.</param>
        /// <returns>The Task.</returns>
        Task PostTesting(string message, DateTime? lastUpdated = null);

        /// <summary>
        /// Posts the terminating.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="lastUpdated">The last updated.</param>
        /// <returns>The Task.</returns>
        Task PostTerminating(string message, DateTime? lastUpdated = null);
    }
}
