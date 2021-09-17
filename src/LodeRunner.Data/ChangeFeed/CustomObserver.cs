// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        /// <param name="cancellationToken">Token to signal that the parition processing is going to finish.</param>
        /// <returns>
        /// A Task to allow asynchronous execution.
        /// </returns>
        public Task ProcessChangesAsync(IChangeFeedObserverContext context, IReadOnlyList<Document> docs, CancellationToken cancellationToken)
        {
            foreach (var document in docs)
            {
                string entityType = document.GetPropertyValue<string>("entityType");

                switch (entityType)
                {
                    case "ClientStatus":
                        Console.WriteLine("Processing entityType, ClientStatus");

                        // TODO: How to handle delegates
                        // App.Config.Cache.ProcessClientStatusChange(document);
                        break;
                    default:
                        Console.WriteLine("Unable to process unaccounted entityType, {0}", entityType);
                        break;
                }
            }

            return Task.CompletedTask;
        }
    }
}
