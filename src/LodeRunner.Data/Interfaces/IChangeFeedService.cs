// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.Core.Models;
using LodeRunner.Data.ChangeFeed;
using Microsoft.Azure.Documents.ChangeFeedProcessor.PartitionManagement;
using static LodeRunner.Services.ChangeFeedService;

namespace LodeRunner.Data.Interfaces
{
    /// <summary>
    /// ChangeFeedService Interface.
    /// </summary>
    public interface IChangeFeedService
    {
        /// <summary>
        /// Runs the change feed processor.
        /// </summary>
        /// <param name="customObserverReadyCallback">The callback when CustomObserver is Ready.</param>
        /// <returns>The IChangeFeedProcessor task.</returns>
        Task<IChangeFeedProcessor> StartChangeFeedProcessor(Action customObserverReadyCallback);

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
