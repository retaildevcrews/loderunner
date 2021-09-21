// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using LodeRunner.Data.ChangeFeed;
using LodeRunner.Data.Interfaces;
using Microsoft.Azure.Documents.ChangeFeedProcessor;
using Microsoft.Azure.Documents.ChangeFeedProcessor.PartitionManagement;
using IChangeFeedObserver = Microsoft.Azure.Documents.ChangeFeedProcessor.FeedProcessing.IChangeFeedObserver;

namespace LodeRunner.Services
{
    /// <summary>
    ///   Change Feed Service.
    /// </summary>
    public abstract class ChangeFeedService : BaseService, IChangeFeedService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeFeedService"/> class.
        /// </summary>
        /// <param name="cosmosDBRepository">The cosmos database repository.</param>
        public ChangeFeedService(ICosmosDBRepository cosmosDBRepository)
            : base(cosmosDBRepository)
        {
            this.CreateProcessor();
        }

        /// <summary>
        /// Process Change EventHandler.
        /// </summary>
        /// <param name="eventArgs">The <see cref="ProcessChangesEventArgs"/> instance containing the event data.</param>
        public delegate void ProcessChangeEventHandler(ProcessChangesEventArgs eventArgs);

        /// <summary>
        /// Gets the name of the change feed lease.
        /// </summary>
        /// <value>
        /// The name of the change feed lease.
        /// </value>
        public abstract string ChangeFeedLeaseName { get; }

        /// <summary>
        /// Gets the name of the host.
        /// </summary>
        /// <value>
        /// The name of the host.
        /// </value>
        public abstract string HostName { get; }

        /// <summary>
        /// Gets or sets the processor.
        /// </summary>
        protected IProcessor Processor { get; set; }

        /// <summary>
        /// Gets the change feed observer.
        /// </summary>
        /// <returns>
        /// The IChangeFeedObserver.
        /// </returns>
        /// <value>
        /// The change feed observer.
        /// </value>
        public abstract IChangeFeedObserver GetChangeFeedObserver();

        /// <summary>
        /// Creates the processor.
        /// </summary>
        public abstract void CreateProcessor();

        /// <summary>
        /// Runs the change feed processor.
        /// </summary>
        /// <param name="observerReadyCallback">The callback when Observer is Ready.</param>
        /// <returns>
        /// The IChangeFeedProcessor task.
        /// </returns>
        public async Task<IChangeFeedProcessor> StartChangeFeedProcessor(Action observerReadyCallback)
        {
            DocumentCollectionInfo feedCollectionInfo = this.CosmosDBRepository.GetNewDocumentCollectionInfo(this.CosmosDBRepository.CollectionName);

            DocumentCollectionInfo leaseCollectionInfo = this.CosmosDBRepository.GetNewDocumentCollectionInfo(this.ChangeFeedLeaseName);

            return await this.Processor.StartAsync(this.HostName, feedCollectionInfo, leaseCollectionInfo, observerReadyCallback);
        }

        /// <summary>
        /// Stops the change feed processor.
        /// </summary>
        public async void StopChangeFeedProcessor()
        {
            await this.Processor.StopAsync();
        }
    }
}
