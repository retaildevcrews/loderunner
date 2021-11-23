// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using LodeRunner.API.Data;
using LodeRunner.API.Interfaces;
using LodeRunner.Data.ChangeFeed;
using Microsoft.Extensions.DependencyInjection;

namespace LodeRunner.API.Services
{
    public class SystemComponentsService : ISystemComponentsService
    {
        private readonly CancellationTokenSource cancelTokenSource;

        private readonly ILRAPIChangeFeedService lrAPIChangeFeedService;

        private readonly ILRAPICache lrAPICache;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemComponentsService"/> class.
        /// </summary>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <param name="lrAPIChangeFeedService">lrAPI ChangeFeed Service.</param>
        /// <param name="lrAPICache">lrAPI Cache.</param>
        public SystemComponentsService(CancellationTokenSource cancellationTokenSource, ILRAPIChangeFeedService lrAPIChangeFeedService, ILRAPICache lrAPICache)
        {
            this.cancelTokenSource = cancellationTokenSource;

            this.lrAPIChangeFeedService = lrAPIChangeFeedService;
            this.lrAPICache = lrAPICache;
        }

        /// <summary>
        /// Initializes the system components.
        /// </summary>
        public void InitializeSystemComponents()
        {
            // Forces the creation of Required System Objects
            ForceToCreateRequiredSystemObjects();

            // Registers the cancellation tokens for services.
            RegisterCancellationTokensForServices();

            // start CosmosDB Change Feed Processor
            GetLRAPIChangeFeedService().StartChangeFeedProcessor(() => EventsSubscription());
        }

        /// <summary>
        /// Registers the cancellation tokens for services.
        /// This method will allows to register multiple Cancellation tokens with different purposes for different Services.
        /// </summary>
        private void RegisterCancellationTokensForServices()
        {
            this.cancelTokenSource.Token.Register(() =>
            {
                GetLRAPIChangeFeedService().StopChangeFeedProcessor();
            });
        }

        /// <summary>
        /// Forces to create required system objects.
        /// </summary>
        private void ForceToCreateRequiredSystemObjects()
        {
            // Cache
            GetLRAPICache();
        }

        /// <summary>
        /// Registers the events.
        /// </summary>
        private void EventsSubscription()
        {
            GetLRAPIChangeFeedService().SubscribeToProcessClientStatusChange(ProcessClientStatusChange);

            GetLRAPIChangeFeedService().SubscribeToProcessLoadClientChange(ProcessLoadClientChange);

            GetLRAPIChangeFeedService().SubscribeToProcessLoadTestConfigChange(ProcessLoadTestConfigChange);

            GetLRAPIChangeFeedService().SubscribeToProcessTestRunChange(ProcessTestRunChange);
        }

        /// <summary>
        /// Processes the client status change.
        /// </summary>
        /// <param name="e">The <see cref="ProcessChangesEventArgs"/> instance containing the event data.</param>
        private void ProcessClientStatusChange(ProcessChangesEventArgs e)
        {
            GetLRAPICache().ProcessClientStatusChange(e.Document);
        }

        /// <summary>
        /// Processes the load client change.
        /// </summary>
        /// <param name="e">The <see cref="ProcessChangesEventArgs"/> instance containing the event data.</param>
        private void ProcessLoadClientChange(ProcessChangesEventArgs e)
        {
            // TODO: how we are going to process a LoadClient Change
        }

        /// <summary>
        /// Processes the load test configuration change.
        /// </summary>
        /// <param name="e">The <see cref="ProcessChangesEventArgs"/> instance containing the event data.</param>
        private void ProcessLoadTestConfigChange(ProcessChangesEventArgs e)
        {
            // TODO: how we are going to process a TestConfig Change?
        }

        /// <summary>
        /// Processes the test run change.
        /// </summary>
        /// <param name="e">The <see cref="ProcessChangesEventArgs"/> instance containing the event data.</param>
        private void ProcessTestRunChange(ProcessChangesEventArgs e)
        {
            // TODO: how we are going to process TestRun Change?
        }

        /// <summary>
        /// Gets the change feed service.
        /// </summary>
        /// <returns>The ChangeFeed Service.</returns>
        private ILRAPIChangeFeedService GetLRAPIChangeFeedService()
        {
            return this.lrAPIChangeFeedService;
        }

        /// <summary>
        /// Gets the cache service.
        /// </summary>
        /// <returns>The Cache Service.</returns>
        private ILRAPICache GetLRAPICache()
        {
            return this.lrAPICache;
        }
    }
}
