// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Ngsa.LodeRunner.DataAccessLayer.Model;

namespace Ngsa.LodeRunner.DataAccessLayer.Interfaces
{
    /// <summary>
    /// ClientStatusService Interface.
    /// </summary>
    public interface IClientStatusService
    {
        /// <summary>
        /// Posts the specified status.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="clientStatus">The ClientStatus entity.</param>
        /// <param name="lastUpdated">The last updated.</param>
        /// <param name="status">The status.</param>
        /// <returns>The Task.</returns>
        Task<ClientStatus> Post(string message, ClientStatus clientStatus, DateTime lastUpdated, ClientStatusType status);

        /// <summary>
        /// Posts the starting.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="lastUpdated">The last updated.</param>
        /// <returns>The Task.</returns>
        Task<ClientStatus> PostStarting(string message, DateTime lastUpdated);

        /// <summary>
        /// Posts the update.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="clientStatus">The client status.</param>
        /// <param name="lastUpdated">The last updated.</param>
        /// <param name="status">The status.</param>
        /// <returns>The Task.</returns>
        Task<ClientStatus> PostUpdate(string message, ClientStatus clientStatus, DateTime lastUpdated, ClientStatusType status);
    }
}
