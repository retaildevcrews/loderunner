// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents.ChangeFeedProcessor;
using Microsoft.Azure.Documents.ChangeFeedProcessor.PartitionManagement;
using static LodeRunner.Data.ChangeFeed.CustomObserver;
using ChangeFeedProcessorBuilder = Microsoft.Azure.Documents.ChangeFeedProcessor.ChangeFeedProcessorBuilder;

namespace LodeRunner.Data.ChangeFeed
{
    /// <summary>
    /// Represents the Processor class.
    /// </summary>
    public class Processor : IDisposable
    {
        /// <summary>
        /// Gets the custom observer factory.
        /// </summary>
        /// <value>
        /// The custom observer factory.
        /// </value>
        private CustomObserverFactory customObserverFactory;

        /// <summary>
        /// Gets the custom observer.
        /// </summary>
        /// <value>
        /// The custom observer.
        /// </value>
        internal CustomObserver CustomObserver => this.customObserverFactory.CustomObserverInstance;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.customObserverFactory = null;
        }

        /// <summary>
        /// Starts the asynchronous.
        /// </summary>
        /// <param name="hostName">Name of the host.</param>
        /// <param name="feedCollectionInfo">The feed collection information.</param>
        /// <param name="leaseCollectionInfo">The lease collection information.</param>
        /// <param name="onCustomObserverReadyCallback">The callback when CustomObserver is Ready.</param>
        /// <returns>The IChangeFeedProcessor.</returns>
        public async Task<IChangeFeedProcessor> StartAsync(string hostName, DocumentCollectionInfo feedCollectionInfo, DocumentCollectionInfo leaseCollectionInfo, Action onCustomObserverReadyCallback)
        {
            this.customObserverFactory = new CustomObserverFactory(onCustomObserverReadyCallback);

            var builder = new ChangeFeedProcessorBuilder();
            var processor = await builder
                .WithHostName(hostName)
                .WithFeedCollection(feedCollectionInfo)
                .WithLeaseCollection(leaseCollectionInfo)
                .WithObserverFactory(this.customObserverFactory)
                .BuildAsync();

            Console.WriteLine("Starting Change Feed Processor....");

            await processor.StartAsync();

            Console.WriteLine("Change Feed Processor started....");
            return processor;
        }
    }
}
