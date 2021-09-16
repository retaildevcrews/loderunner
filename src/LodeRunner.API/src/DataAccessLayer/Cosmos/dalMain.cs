// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LodeRunner.API.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;

namespace LodeRunner.API.DataAccessLayer
{
    /// <summary>
    /// Data Access Layer for CosmosDB
    /// </summary>
    public partial class CosmosDal : IDAL, IDisposable
    {
        private readonly CosmosConfig cosmosDetails;
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDal"/> class.
        /// </summary>
        /// <param name="secrets">Cosmos connection info</param>
        /// <param name="config">App config</param>
        public CosmosDal(Secrets secrets, Config config)
        {
            if (secrets == null)
            {
                throw new ArgumentNullException(nameof(secrets));
            }

            cosmosDetails = new CosmosConfig
            {
                CosmosCollection = secrets.CosmosCollection,
                CosmosDatabase = secrets.CosmosDatabase,
                CosmosKey = secrets.CosmosKey,
                CosmosUrl = secrets.CosmosServer,
                Retries = config.Retries,
                Timeout = config.Timeout,
            };

            // create the CosmosDB client and source container
            cosmosDetails.Client = new CosmosClient(secrets.CosmosServer, secrets.CosmosKey, cosmosDetails.CosmosClientOptions);
            cosmosDetails.Container = cosmosDetails.Client.GetContainer(secrets.CosmosDatabase, secrets.CosmosCollection);
        }

        // implement IDisposable
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
                    // Add cleanup code
                }

                disposedValue = true;
            }
        }
    }
}
