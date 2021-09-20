// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using LodeRunner.Data.ChangeFeed;
using LodeRunner.Data.Interfaces;
using Microsoft.Azure.Documents.ChangeFeedProcessor;
using Microsoft.Azure.Documents.ChangeFeedProcessor.PartitionManagement;
using static LodeRunner.Data.ChangeFeed.CustomObserver;

namespace LodeRunner.Services
{
    /// <summary>
    ///   Change Feed Service.
    /// </summary>
    public class ChangeFeedService : BaseService, IChangeFeedService
    {
        private readonly Processor processor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeFeedService"/> class.
        /// </summary>
        /// <param name="cosmosDBRepository">The cosmos database repository.</param>
        public ChangeFeedService(ICosmosDBRepository cosmosDBRepository)
            : base(cosmosDBRepository)
        {
            this.processor = new Processor();
        }

        /// <summary>
        /// Process Change EventHandler.
        /// </summary>
        /// <param name="eventArgs">The <see cref="ProcessChangesEventArgs"/> instance containing the event data.</param>
        public delegate void ProcessChangeEventHandler(ProcessChangesEventArgs eventArgs);

        /// <summary>
        /// Runs the change feed processor.
        /// </summary>
        /// <param name="customObserverReadyCallback">The callback when CustomObserver is Ready.</param>
        /// <returns>
        /// The IChangeFeedProcessor task.
        /// </returns>
        public async Task<IChangeFeedProcessor> StartChangeFeedProcessor(Action customObserverReadyCallback)
        {
            const string ChangeFeedLeaseName = "RRAPI";

            DocumentCollectionInfo feedCollectionInfo = this.CosmosDBRepository.GetNewDocumentCollectionInfo(this.CosmosDBRepository.CollectionName);

            DocumentCollectionInfo leaseCollectionInfo = this.CosmosDBRepository.GetNewDocumentCollectionInfo(ChangeFeedLeaseName);

            return await this.processor.StartAsync($"Host - {Guid.NewGuid()}", feedCollectionInfo, leaseCollectionInfo, customObserverReadyCallback);
        }

        /// <summary>
        /// Subscribes to process test run change.
        /// </summary>
        /// <param name="eventHandler">The event handler.</param>
        public void SubscribeToProcessTestRunChange(ProcessChangeEventHandler eventHandler)
        {
            this.processor.CustomObserver.ProcessTestRunChange += eventHandler;
        }

        /// <summary>
        /// Subscribes to process load test configuration change.
        /// </summary>
        /// <param name="eventHandler">The event handler.</param>
        public void SubscribeToProcessLoadTestConfigChange(ProcessChangeEventHandler eventHandler)
        {
            this.processor.CustomObserver.ProcessLoadTestConfigChange += eventHandler;
        }

        /// <summary>
        /// Subscribes to process load client change.
        /// </summary>
        /// <param name="eventHandler">The event handler.</param>
        public void SubscribeToProcessLoadClientChange(ProcessChangeEventHandler eventHandler)
        {
            this.processor.CustomObserver.ProcessLoadClientChange += eventHandler;
        }

        /// <summary>
        /// Subscribes to process client status change.
        /// </summary>
        /// <param name="eventHandler">The event handler.</param>
        public void SubscribeToProcessClientStatusChange(ProcessChangeEventHandler eventHandler)
        {
            this.processor.CustomObserver.ProcessClientStatusChange += eventHandler;
        }
    }
}
