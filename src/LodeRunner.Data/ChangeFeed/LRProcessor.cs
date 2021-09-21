// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using LodeRunner.Data.Interfaces;
using Microsoft.Azure.Documents.ChangeFeedProcessor;
using Microsoft.Azure.Documents.ChangeFeedProcessor.PartitionManagement;
using ChangeFeedProcessorBuilder = Microsoft.Azure.Documents.ChangeFeedProcessor.ChangeFeedProcessorBuilder;

namespace LodeRunner.Data.ChangeFeed
{
    /// <summary>
    /// Represents the LRAPI Processor class.
    /// </summary>
    public sealed class LRProcessor : IProcessor, IDisposable
    {
        private bool processorStarted = false;

        /// <summary>
        /// Gets the change feed processor.
        /// </summary>
        /// <value>
        /// The change feed processor.
        /// </value>
        public IChangeFeedProcessor ChangeFeedProcessor { get; private set; }

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
        public LRObserver LRAPIObserver => (LRObserver)this.ObserverFactory.GetObserverInstance();

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
            if (this.processorStarted)
            {
                return this.ChangeFeedProcessor;
            }

            await this.InitSystemObjectsAsync(hostName, feedCollectionInfo, leaseCollectionInfo, observerIsReadyCallback);

            Console.WriteLine("Starting Change Feed Processor....");

            await this.ChangeFeedProcessor.StartAsync();

            this.processorStarted = true;

            Console.WriteLine("Change Feed Processor started....");

            return this.ChangeFeedProcessor;
        }

        /// <summary>
        /// Stops the asynchronous.
        /// </summary>
        /// <returns>The Task.</returns>
        public async Task StopAsync()
        {
            await this.ChangeFeedProcessor?.StopAsync();
            this.processorStarted = false;
        }

        /// <summary>
        /// Initializes the system objects asynchronous.
        /// </summary>
        /// <param name="hostName">Name of the host.</param>
        /// <param name="feedCollectionInfo">The feed collection information.</param>
        /// <param name="leaseCollectionInfo">The lease collection information.</param>
        /// <param name="observerIsReadyCallback">The observer is ready callback.</param>
        /// <returns>The Task.</returns>
        public async Task InitSystemObjectsAsync(string hostName, DocumentCollectionInfo feedCollectionInfo, DocumentCollectionInfo leaseCollectionInfo, Action observerIsReadyCallback)
        {
            if (this.ObserverFactory == null)
            {
                this.ObserverFactory = new LRObserverFactory(observerIsReadyCallback);
            }

            if (this.ChangeFeedProcessor == null)
            {
                var builder = new ChangeFeedProcessorBuilder();

                this.ChangeFeedProcessor = await builder
                        .WithHostName(hostName)
                        .WithFeedCollection(feedCollectionInfo)
                        .WithLeaseCollection(leaseCollectionInfo)
                        .WithObserverFactory(this.ObserverFactory)
                        .BuildAsync();
            }
        }
    }
}
