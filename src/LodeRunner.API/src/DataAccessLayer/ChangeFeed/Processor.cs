// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.ChangeFeedProcessor;
using Microsoft.Azure.Documents.ChangeFeedProcessor.PartitionManagement;
using ChangeFeedProcessorBuilder = Microsoft.Azure.Documents.ChangeFeedProcessor.ChangeFeedProcessorBuilder;

namespace LodeRunner.API.Application.ChangeFeed
{
    public class Processor
    {
        public static async Task<IChangeFeedProcessor> RunAsync(string hostName, DocumentCollectionInfo feedCollectionInfo, DocumentCollectionInfo leaseCollectionInfo)
        {
            var builder = new ChangeFeedProcessorBuilder();
            var processor = await builder
                .WithHostName(hostName)
                .WithFeedCollection(feedCollectionInfo)
                .WithLeaseCollection(leaseCollectionInfo)
                .WithObserver<CustomObserver>()
                .BuildAsync();

            Console.WriteLine("Starting Change Feed Processor....");
            await processor.StartAsync();
            Console.WriteLine("Change Feed Processor started....");
            return processor;
        }
    }
}
