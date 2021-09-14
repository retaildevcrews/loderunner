// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.ChangeFeedProcessor.FeedProcessing;

namespace LodeRunner.API.ChangeFeed
{
    public class CustomObserver : IChangeFeedObserver
    {
        public Task CloseAsync(IChangeFeedObserverContext context, ChangeFeedObserverCloseReason reason)
        {
            return Task.CompletedTask;  // Note: requires targeting .Net 4.6+.
        }

        public Task OpenAsync(IChangeFeedObserverContext context)
        {
            return Task.CompletedTask;
        }

        public Task ProcessChangesAsync(IChangeFeedObserverContext context, IReadOnlyList<Document> docs, CancellationToken cancellationToken)
        {
            foreach (var document in docs)
            {
                string entityType = document.GetPropertyValue<string>("entityType");

                switch (entityType)
                {
                    case "ClientStatus":
                        Console.WriteLine("Processing entityType, ClientStatus");
                        App.Config.Cache.ProcessClientStatusChange(document);
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
