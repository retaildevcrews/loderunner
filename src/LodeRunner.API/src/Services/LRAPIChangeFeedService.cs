// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using LodeRunner.API.Interfaces;
using LodeRunner.Data.ChangeFeed;
using LodeRunner.Data.Interfaces;
using LodeRunner.Services;
using IChangeFeedObserver = Microsoft.Azure.Documents.ChangeFeedProcessor.FeedProcessing.IChangeFeedObserver;

namespace LodeRunner.API.Services
{
    /// <summary>
    /// Implements the Relay Runner API Change Feed Service
    /// </summary>
    /// <seealso cref="LodeRunner.Services.ChangeFeedService" />
    public class LRAPIChangeFeedService : ChangeFeedService, ILRAPIChangeFeedService
    {
        public LRAPIChangeFeedService(ICosmosDBRepository cosmosDBRepository)
           : base(cosmosDBRepository)
        {
        }

        public override string ChangeFeedLeaseName => "LRAPI";

        public override string HostName => $"Host - {Guid.NewGuid()}";

        public LRAPIObserver ChangeFeedObserver
        {
            get
            {
                return (LRAPIObserver)GetChangeFeedObserver();
            }
        }

        /// <summary>
        /// Gets the change feed observer.
        /// </summary>
        /// <returns>
        /// The IChangeFeedObserver.
        /// </returns>
        /// <value>
        /// The change feed observer.
        /// </value>
        public override IChangeFeedObserver GetChangeFeedObserver()
        {
            return this.Processor.ObserverFactory.GetObserverInstance();
        }

        /// <summary>
        /// Creates the processor.
        /// </summary>
        public override void CreateProcessor()
        {
            this.Processor = new LRAPIProcessor();
        }

        /// <summary>
        /// Subscribes to process test run change.
        /// </summary>
        /// <param name="eventHandler">The event handler.</param>
        public void SubscribeToProcessTestRunChange(ProcessChangeEventHandler eventHandler)
        {
            this.ChangeFeedObserver.ProcessTestRunChange += eventHandler;
        }

        /// <summary>
        /// Subscribes to process load test configuration change.
        /// </summary>
        /// <param name="eventHandler">The event handler.</param>
        public void SubscribeToProcessLoadTestConfigChange(ProcessChangeEventHandler eventHandler)
        {
            this.ChangeFeedObserver.ProcessLoadTestConfigChange += eventHandler;
        }

        /// <summary>
        /// Subscribes to process load client change.
        /// </summary>
        /// <param name="eventHandler">The event handler.</param>
        public void SubscribeToProcessLoadClientChange(ProcessChangeEventHandler eventHandler)
        {
            this.ChangeFeedObserver.ProcessLoadClientChange += eventHandler;
        }

        /// <summary>
        /// Subscribes to process client status change.
        /// </summary>
        /// <param name="eventHandler">The event handler.</param>
        public void SubscribeToProcessClientStatusChange(ProcessChangeEventHandler eventHandler)
        {
            this.ChangeFeedObserver.ProcessClientStatusChange += eventHandler;
        }
    }
}
