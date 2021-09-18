﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.Core.Extensions;
using LodeRunner.Core.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.ChangeFeedProcessor.FeedProcessing;

namespace LodeRunner.Data.ChangeFeed
{
    /// <summary>
    /// Custom Observer.
    /// </summary>
    /// <seealso cref="Microsoft.Azure.Documents.ChangeFeedProcessor.FeedProcessing.IChangeFeedObserver" />
    public class CustomObserver : IChangeFeedObserver
    {
        /// <summary>
        /// Process Change EventHandler.
        /// </summary>
        /// <param name="eventArgs">The <see cref="ProcessChangesEventArgs"/> instance containing the event data.</param>
        public delegate void ProcessChangeEventHandler(ProcessChangesEventArgs eventArgs);

        /// <summary>
        /// Occurs when [on process load client change].
        /// </summary>
        public event ProcessChangeEventHandler OnProcessLoadClientChange;

        /// <summary>
        /// Occurs when [on process client status change].
        /// </summary>
        public event ProcessChangeEventHandler OnProcessClientStatusChange;

        /// <summary>
        /// Occurs when [on process load test configuration change].
        /// </summary>
        public event ProcessChangeEventHandler OnProcessLoadTestConfigChange;

        /// <summary>
        /// Occurs when [on process test run change].
        /// </summary>
        public event ProcessChangeEventHandler OnProcessTestRunChange;

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
                EntityType entityType = document.GetPropertyValue<string>("entityType").As<EntityType>(EntityType.Unassigned);

                switch (entityType)
                {
                    case EntityType.ClientStatus:
                        Console.WriteLine("Processing entityType, ClientStatus");

                        this.ProcessClientStatusChange(new ProcessChangesEventArgs { LastUpdate = DateTime.UtcNow, Document = document });

                        break;

                    case EntityType.LoadClient:
                        Console.WriteLine("Processing entityType, LoadClient");

                        this.ProcessLoadClientChange(new ProcessChangesEventArgs { LastUpdate = DateTime.UtcNow, Document = document });

                        break;

                    case EntityType.LoadTestConfig:
                        Console.WriteLine("Processing entityType, LoadTestConfig");

                        this.ProcessLoadTestConfigChange(new ProcessChangesEventArgs { LastUpdate = DateTime.UtcNow, Document = document });

                        break;

                    case EntityType.TestRun:
                        Console.WriteLine("Processing entityType, TestRun");

                        this.ProcessTestRunChange(new ProcessChangesEventArgs { LastUpdate = DateTime.UtcNow, Document = document });

                        break;
                    default:
                        Console.WriteLine("Unable to process unaccounted entityType, {0}", entityType);
                        break;
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Raises the <see cref="E:OnProcessTestRunChange" /> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="ProcessChangesEventArgs"/> instance containing the event data.</param>
        private void ProcessTestRunChange(ProcessChangesEventArgs eventArgs)
        {
            this.OnProcessTestRunChange?.Invoke(eventArgs);
        }

        /// <summary>
        /// Raises the <see cref="E:OnProcessLoadTestConfigChange" /> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="ProcessChangesEventArgs"/> instance containing the event data.</param>
        private void ProcessLoadTestConfigChange(ProcessChangesEventArgs eventArgs)
        {
            this.OnProcessLoadTestConfigChange?.Invoke(eventArgs);
        }

        /// <summary>
        /// Raises the <see cref="E:ProcessLoadClientChange" /> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="ProcessChangesEventArgs"/> instance containing the event data.</param>
        private void ProcessLoadClientChange(ProcessChangesEventArgs eventArgs)
        {
            this.OnProcessLoadClientChange?.Invoke(eventArgs);
        }

        /// <summary>
        /// Raises the <see cref="E:OnProcessClientStatusChange" /> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="ProcessChangesEventArgs"/> instance containing the event data.</param>
        private void ProcessClientStatusChange(ProcessChangesEventArgs eventArgs)
        {
            this.OnProcessClientStatusChange?.Invoke(eventArgs);
        }
    }
}
