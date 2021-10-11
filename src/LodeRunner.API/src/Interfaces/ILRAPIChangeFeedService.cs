// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LodeRunner.Data.Interfaces;
using static LodeRunner.Services.ChangeFeedService;

namespace LodeRunner.API.Interfaces
{
    /// <summary>
    /// Interface for the LodeRunner.API changefeed service.
    /// </summary>
    internal interface ILRAPIChangeFeedService : IChangeFeedService
    {
        /// <summary>
        /// Subscribes to process test run change.
        /// </summary>
        /// <param name="eventHandler">The event handler.</param>
        void SubscribeToProcessTestRunChange(ProcessChangeEventHandler eventHandler);

        /// <summary>
        /// Subscribes to process load test configuration change.
        /// </summary>
        /// <param name="eventHandler">The event handler.</param>
        void SubscribeToProcessLoadTestConfigChange(ProcessChangeEventHandler eventHandler);

        /// <summary>
        /// Subscribes to process load client change.
        /// </summary>
        /// <param name="eventHandler">The event handler.</param>
        void SubscribeToProcessLoadClientChange(ProcessChangeEventHandler eventHandler);

        /// <summary>
        /// Subscribes to process client status change.
        /// </summary>
        /// <param name="eventHandler">The event handler.</param>
        void SubscribeToProcessClientStatusChange(ProcessChangeEventHandler eventHandler);
    }
}
