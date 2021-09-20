// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using LodeRunner.Data.Interfaces;
using Microsoft.Azure.Documents.ChangeFeedProcessor;
using Microsoft.Azure.Documents.ChangeFeedProcessor.PartitionManagement;
using ChangeFeedProcessorBuilder = Microsoft.Azure.Documents.ChangeFeedProcessor.ChangeFeedProcessorBuilder;
using IChangeFeedObserverFactory = Microsoft.Azure.Documents.ChangeFeedProcessor.FeedProcessing.IChangeFeedObserverFactory;

namespace LodeRunner.Data.ChangeFeed
{
    /// <summary>
    /// Represents the LRAPI Processor class.
    /// </summary>
    public sealed class LRAPIProcessor : IProcessor, IDisposable
    {
        private IChangeFeedProcessor changeFeedProcessor;

        /// <summary>
        /// Gets the base observer factory.
        /// </summary>
        /// <value>
        /// The observer factory.
        /// </value>
        public IBaseObserverFactory ObserverFactory { get; private set; }

        /// <summary>
        /// Gets the LRAPI observer.
        /// </summary>
        /// <value>
        /// The Relay Runner observer.
        /// </value>
        public LRAPIObserver LRAPIObserver => (LRAPIObserver)this.ObserverFactory.GetObserverInstance();

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.ObserverFactory = null;
        }

        /// <summary>
        /// Starts the asynchronous.
        /// </summary>
        /// <param name="hostName">Name of the host.</param>
        /// <param name="feedCollectionInfo">The feed collection information.</param>
        /// <param name="leaseCollectionInfo">The lease collection information.</param>
        /// <param name="observerIsReadyCallback">The callback when Observer is Ready.</param>
        /// <returns>The IChangeFeedProcessor.</returns>
        public async Task<IChangeFeedProcessor> StartAsync(string hostName, DocumentCollectionInfo feedCollectionInfo, DocumentCollectionInfo leaseCollectionInfo, Action observerIsReadyCallback)
        {
            this.ObserverFactory = new LRAPIObserverFactory(observerIsReadyCallback);

            var builder = new ChangeFeedProcessorBuilder();
            this.changeFeedProcessor = await builder
                .WithHostName(hostName)
                .WithFeedCollection(feedCollectionInfo)
                .WithLeaseCollection(leaseCollectionInfo)
                .WithObserverFactory(this.ObserverFactory)
                .BuildAsync();

            Console.WriteLine("Starting Change Feed Processor....");

            await this.changeFeedProcessor.StartAsync();

            Console.WriteLine("Change Feed Processor started....");
            return this.changeFeedProcessor;
        }

        /// <summary>
        /// Stops the asynchronous.
        /// </summary>
        /// <returns>The Task.</returns>
        public async Task StopAsync()
        {
            await this.changeFeedProcessor.StopAsync();
        }
    }
}
