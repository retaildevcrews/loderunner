// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Ngsa.LodeRunner.DataAccessLayer
{
    /// <summary>
    /// Data Access Layer for CosmosDB
    /// </summary>
    public partial class CosmosDal : IDAL, IDisposable
    {
        private const string ColumnIdValue = "action";
        private const string PartitionKeyValue = "0";
        private readonly MemoryCache cache = new ("cache");
        private readonly CosmosConfig cosmosDetails;
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDal"/> class.
        /// </summary>
        /// <param name="config">App config</param>
        public CosmosDal(Config config)
        {
            if (config.Secrets == null)
            {
                throw new ArgumentNullException("Secrets cannot be null", nameof(config.Secrets));
            }

            cosmosDetails = new CosmosConfig
            {
                CosmosCollection = config.Secrets.CosmosCollection,
                CosmosDatabase = config.Secrets.CosmosDatabase,
                CosmosKey = config.Secrets.CosmosKey,
                CosmosUrl = config.Secrets.CosmosServer,
                Retries = config.Retries,
                Timeout = config.CosmosTimeout,
            };

            // create the CosmosDB client and container
            cosmosDetails.Client = OpenAndTestCosmosClient(config.Secrets.CosmosServer, config.Secrets.CosmosKey, config.Secrets.CosmosDatabase, config.Secrets.CosmosCollection).GetAwaiter().GetResult();
            cosmosDetails.Container = cosmosDetails.Client.GetContainer(config.Secrets.CosmosDatabase, config.Secrets.CosmosCollection);
        }

        /// <summary>
        /// Open and test the Cosmos Client / Container / Query
        /// </summary>
        /// <param name="cosmosServer">Cosmos URL</param>
        /// <param name="cosmosKey">Cosmos Key</param>
        /// <param name="cosmosDatabase">Cosmos Database</param>
        /// <param name="cosmosCollection">Cosmos Collection</param>
        /// <returns>An open and validated CosmosClient</returns>
        private async Task<CosmosClient> OpenAndTestCosmosClient(string cosmosServer, string cosmosKey, string cosmosDatabase, string cosmosCollection)
        {
            // validate required parameters
            if (cosmosServer == null)
            {
                throw new ArgumentNullException(nameof(cosmosServer));
            }

            if (string.IsNullOrWhiteSpace(cosmosKey))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, $"CosmosKey not set correctly {cosmosKey}"));
            }

            if (string.IsNullOrWhiteSpace(cosmosDatabase))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, $"CosmosDatabase not set correctly {cosmosDatabase}"));
            }

            if (string.IsNullOrWhiteSpace(cosmosCollection))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, $"CosmosCollection not set correctly {cosmosCollection}"));
            }

            // open and test a new client / container
            CosmosClient cosmosClient = new (cosmosServer, cosmosKey, cosmosDetails.CosmosClientOptions);
            Container con = cosmosClient.GetContainer(cosmosDatabase, cosmosCollection);
            await con.ReadItemAsync<dynamic>(ColumnIdValue, new PartitionKey(PartitionKeyValue)).ConfigureAwait(false);

            //TODO: Design Review - Add internal notification on successful configuration.

            return cosmosClient;
        }

        // implement IDisposable
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:Elements should be ordered by access", Justification = "clarity")]
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (cache != null)
                    {
                        cache.Dispose();
                    }
                }

                disposedValue = true;
            }
        }
    }
}
