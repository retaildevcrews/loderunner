// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.ChangeFeedProcessor;
using Microsoft.Azure.Documents.ChangeFeedProcessor.PartitionManagement;

namespace LodeRunner.Data.Interfaces
{
    /// <summary>
    /// The Processor Interface.
    /// </summary>
    public interface IProcessor
    {
        /// <summary>
        /// Gets the change feed processor.
        /// </summary>
        /// <value>
        /// The change feed processor.
        /// </value>
        IChangeFeedProcessor ChangeFeedProcessor { get; }

        /// <summary>
        /// Gets the observer factory.
        /// </summary>
        /// <value>
        /// The observer factory.
        /// </value>
        IBaseObserverFactory ObserverFactory { get; }

        /// <summary>
        /// Starts the asynchronous.
        /// </summary>
        /// <param name="hostName">Name of the host.</param>
        /// <param name="feedCollectionInfo">The feed collection information.</param>
        /// <param name="leaseCollectionInfo">The lease collection information.</param>
        /// <param name="observerIsReadyCallback">The observer is ready callback.</param>
        /// <returns>The IChangeFeedProcessor.</returns>
        Task<IChangeFeedProcessor> StartAsync(string hostName, DocumentCollectionInfo feedCollectionInfo, DocumentCollectionInfo leaseCollectionInfo, Action observerIsReadyCallback);

        /// <summary>
        /// Initializes the system objects asynchronous.
        /// </summary>
        /// <param name="hostName">Name of the host.</param>
        /// <param name="feedCollectionInfo">The feed collection information.</param>
        /// <param name="leaseCollectionInfo">The lease collection information.</param>
        /// <param name="observerIsReadyCallback">The observer is ready callback.</param>
        /// <returns>The Task.</returns>
        Task InitSystemObjectsAsync(string hostName, DocumentCollectionInfo feedCollectionInfo, DocumentCollectionInfo leaseCollectionInfo, Action observerIsReadyCallback);

        /// <summary>
        /// Stops the asynchronous.
        /// </summary>
        /// <returns>The task.</returns>
        Task StopAsync();
    }
}
