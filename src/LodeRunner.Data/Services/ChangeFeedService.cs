// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using LodeRunner.Data.ChangeFeed;
using LodeRunner.Data.Interfaces;
using Microsoft.Azure.Documents.ChangeFeedProcessor;
using Microsoft.Azure.Documents.ChangeFeedProcessor.PartitionManagement;

namespace LodeRunner.Services
{
    /// <summary>
    ///   Change Feed Service.
    /// </summary>
    public class ChangeFeedService : BaseService, IChangeFeedService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeFeedService"/> class.
        /// </summary>
        /// <param name="cosmosDBRepository">The cosmos database repository.</param>
        public ChangeFeedService(ICosmosDBRepository cosmosDBRepository)
            : base(cosmosDBRepository)
        {
        }

        /// <summary>
        /// Runs the change feed processor.
        /// </summary>
        /// <returns>
        /// The IChangeFeedProcessor task.
        /// </returns>
        public async Task<IChangeFeedProcessor> RunChangeFeedProcessor()
        {
            const string ChangeFeedLeaseName = "RRAPI";

            DocumentCollectionInfo feedCollectionInfo = this.CosmosDBRepository.GetNewDocumentCollectionInfo(this.CosmosDBRepository.CollectionName);

            DocumentCollectionInfo leaseCollectionInfo = this.CosmosDBRepository.GetNewDocumentCollectionInfo(ChangeFeedLeaseName);

            return await Processor.RunAsync($"Host - {Guid.NewGuid()}", feedCollectionInfo, leaseCollectionInfo);
        }
    }
}
