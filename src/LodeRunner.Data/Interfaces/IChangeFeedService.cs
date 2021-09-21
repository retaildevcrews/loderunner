// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.ChangeFeedProcessor.PartitionManagement;
using IChangeFeedObserver = Microsoft.Azure.Documents.ChangeFeedProcessor.FeedProcessing.IChangeFeedObserver;

namespace LodeRunner.Data.Interfaces
{
    /// <summary>
    /// ChangeFeedService Interface.
    /// </summary>
    public interface IChangeFeedService
    {
        /// <summary>
        /// Gets the name of the change feed lease.
        /// </summary>
        /// <value>
        /// The name of the change feed lease.
        /// </value>
        string ChangeFeedLeaseName { get; }

        /// <summary>
        /// Gets the name of the host.
        /// </summary>
        /// <value>
        /// The name of the host.
        /// </value>
        string HostName { get; }

        /// <summary>
        /// Gets the change feed observer.
        /// </summary>
        /// <value>
        /// The change feed observer.
        /// </value>
        /// <returns> The IChangeFeedObserver.</returns>
        IChangeFeedObserver GetChangeFeedObserver();

        /// <summary>
        /// Creates the processor.
        /// </summary>
        void CreateProcessor();

        /// <summary>
        /// Runs the change feed processor.
        /// </summary>
        /// <param name="observerReadyCallback">The callback when Observer is Ready.</param>
        /// <returns>The IChangeFeedProcessor task.</returns>
        Task<IChangeFeedProcessor> StartChangeFeedProcessor(Action observerReadyCallback);

        /// <summary>
        /// Stops the change feed processor.
        /// </summary>
        void StopChangeFeedProcessor();
    }
}
