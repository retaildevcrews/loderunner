// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.Core.Extensions;
using LodeRunner.Core.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.ChangeFeedProcessor.FeedProcessing;
using static LodeRunner.Services.ChangeFeedService;

namespace LodeRunner.Data.ChangeFeed
{
    /// <summary>
    /// Relay Runner Observer.
    /// </summary>
    /// <seealso cref="Microsoft.Azure.Documents.ChangeFeedProcessor.FeedProcessing.IChangeFeedObserver" />
    public sealed class LRObserver : IChangeFeedObserver
    {
        /// <summary>
        /// Occurs when [on process load client change].
        /// </summary>
        public event ProcessChangeEventHandler ProcessLoadClientChange;

        /// <summary>
        /// Occurs when [on process client status change].
        /// </summary>
        public event ProcessChangeEventHandler ProcessClientStatusChange;

        /// <summary>
        /// Occurs when [on process load test configuration change].
        /// </summary>
        public event ProcessChangeEventHandler ProcessLoadTestConfigChange;

        /// <summary>
        /// Occurs when [on process test run change].
        /// </summary>
        public event ProcessChangeEventHandler ProcessTestRunChange;

        /// <summary>
        /// This is called when change feed observer is closed.
        /// </summary>
        /// <param name="context">The context specifying partition for this observer, etc.</param>
        /// <param name="reason">Specifies the reason the observer is closed.</param>
        /// <returns>
        /// A Task to allow asynchronous execution.
        /// </returns>
        public Task CloseAsync(IChangeFeedObserverContext context, ChangeFeedObserverCloseReason reason)
        {
            return Task.CompletedTask;  // Note: requires targeting .Net 4.6+.
        }

        /// <summary>
        /// This is called when change feed observer is opened.
        /// </summary>
        /// <param name="context">The context specifying partition for this observer, etc.</param>
        /// <returns>
        /// A Task to allow asynchronous execution.
        /// </returns>
        public Task OpenAsync(IChangeFeedObserverContext context)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// This is called when document changes are available on change feed.
        /// </summary>
        /// <param name="context">The context specifying partition for this change event, etc.</param>
        /// <param name="docs">The documents changed.</param>
        /// <param name="cancellationToken">Token to signal that the partition processing is going to finish.</param>
        /// <returns>
        /// A Task to allow asynchronous execution.
        /// </returns>
        public Task ProcessChangesAsync(IChangeFeedObserverContext context, IReadOnlyList<Document> docs, CancellationToken cancellationToken)
        {
            foreach (var document in docs)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                EntityType entityType = document.GetPropertyValue<string>("entityType").As<EntityType>(EntityType.Unassigned, true);

                switch (entityType)
                {
                    case EntityType.ClientStatus:
                        Debug.WriteLine("Processing entityType, ClientStatus");

                        this.OnProcessClientStatusChange(new ProcessChangesEventArgs { LastUpdate = DateTime.UtcNow, Document = document });

                        break;

                    case EntityType.LoadClient:
                        Debug.WriteLine("Processing entityType, LoadClient");

                        this.OnProcessLoadClientChange(new ProcessChangesEventArgs { LastUpdate = DateTime.UtcNow, Document = document });

                        break;

                    case EntityType.LoadTestConfig:
                        Debug.WriteLine("Processing entityType, LoadTestConfig");

                        this.OnProcessLoadTestConfigChange(new ProcessChangesEventArgs { LastUpdate = DateTime.UtcNow, Document = document });

                        break;

                    case EntityType.TestRun:
                        Debug.WriteLine("Processing entityType, TestRun");

                        this.OnProcessTestRunChange(new ProcessChangesEventArgs { LastUpdate = DateTime.UtcNow, Document = document });

                        break;
                    default:
                        Debug.WriteLine("Unable to process unaccounted entityType, {0}", entityType);
                        break;
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Raises the <see cref="E:OnProcessTestRunChange" /> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="ProcessChangesEventArgs"/> instance containing the event data.</param>
        private void OnProcessTestRunChange(ProcessChangesEventArgs eventArgs)
        {
            this.ProcessTestRunChange?.Invoke(eventArgs);
        }

        /// <summary>
        /// Raises the <see cref="E:OnProcessLoadTestConfigChange" /> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="ProcessChangesEventArgs"/> instance containing the event data.</param>
        private void OnProcessLoadTestConfigChange(ProcessChangesEventArgs eventArgs)
        {
            this.ProcessLoadTestConfigChange?.Invoke(eventArgs);
        }

        /// <summary>
        /// Raises the <see cref="E:ProcessLoadClientChange" /> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="ProcessChangesEventArgs"/> instance containing the event data.</param>
        private void OnProcessLoadClientChange(ProcessChangesEventArgs eventArgs)
        {
            this.ProcessLoadClientChange?.Invoke(eventArgs);
        }

        /// <summary>
        /// Raises the <see cref="E:OnProcessClientStatusChange" /> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="ProcessChangesEventArgs"/> instance containing the event data.</param>
        private void OnProcessClientStatusChange(ProcessChangesEventArgs eventArgs)
        {
            this.ProcessClientStatusChange?.Invoke(eventArgs);
        }
    }
}
