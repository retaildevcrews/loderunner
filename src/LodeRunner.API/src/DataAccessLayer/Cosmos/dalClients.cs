// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LodeRunner.API.Models;
using LodeRunner.API.Models.Enum;
using Microsoft.Azure.Cosmos;

namespace LodeRunner.API.DataAccessLayer
{
    /// <summary>
    /// Data Access Layer for CosmosDB
    /// </summary>
    public partial class CosmosDal
    {
        /// <summary>
        /// Retrieves ClientStatus from CosmosDB and returns Clients (flattened ClientStatus)
        /// Uses the CosmosDB single document read API which is 1 RU if less than 1K doc size
        /// </summary>
        /// <returns>array of Clients</returns>
        public async Task<IEnumerable<ClientStatus>> GetClientStatusesAsync()
        {
            // create query
            QueryDefinition sql = new QueryDefinition("select * from clientStatus cs where cs.entityType = @entityType").WithParameter("@entityType", EntityType.ClientStatus.ToString());

            // run query
            FeedIterator<ClientStatus> query = cosmosDetails.Container.GetItemQueryIterator<ClientStatus>(sql);

            // get results
            List<ClientStatus> results = new ();

            while (query.HasMoreResults)
            {
                foreach (ClientStatus clientStatus in await query.ReadNextAsync().ConfigureAwait(false))
                {
                    results.Add(clientStatus);
                }
            }

            return results;
        }

        /// <summary>
        /// Retrieve a single Client from CosmosDB by clientStatusId
        ///
        /// Uses the CosmosDB single document read API which is 1 RU if less than 1K doc size
        ///
        /// Throws an exception if not found
        /// </summary>
        /// <param name="clientStatusId">ClientStatus ID</param>
        /// <returns>Client object</returns>
        public async Task<ClientStatus> GetClientStatusByClientStatusIdAsync(string clientStatusId)
        {
            if (string.IsNullOrWhiteSpace(clientStatusId))
            {
                throw new ArgumentNullException(nameof(clientStatusId));
            }

            // get the partition key for ClientStatus
            // note: if the key cannot be determined from the ID, ReadDocumentAsync cannot be used.
            // ComputePartitionKey will throw an ArgumentException if the clientStatusId isn't valid
            // get a load client by ID

            return await cosmosDetails.Container
                .ReadItemAsync<ClientStatus>(clientStatusId, new PartitionKey(ClientStatus.ComputePartitionKey()))
                .ConfigureAwait(false);
        }
    }
}
