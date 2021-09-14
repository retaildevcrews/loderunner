// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Azure.Cosmos;

namespace LodeRunner.API.Application.DataAccessLayer
{
    /// <summary>
    /// Internal class for Cosmos config
    /// </summary>
    internal class CosmosConfig
    {
        // member variables
        private CosmosClientOptions cosmosClientOptions;

        public CosmosClient Client { get; set; }
        public Container Container { get; set; }

        // default values for Cosmos Options
        public int Timeout { get; set; } = 30;
        public int Retries { get; set; } = 5;

        // Cosmos connection fields
        public string CosmosUrl { get; set; }
        public string CosmosKey { get; set; }
        public string CosmosDatabase { get; set; }
        public string CosmosCollection { get; set; }

        // default protocol is tcp, default connection mode is direct
        public CosmosClientOptions CosmosClientOptions
        {
            get
            {
                if (cosmosClientOptions == null)
                {
                    cosmosClientOptions = new ()
                    {
                        RequestTimeout = TimeSpan.FromSeconds(Timeout),
                        MaxRetryAttemptsOnRateLimitedRequests = Retries,
                        MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(Timeout),
                        SerializerOptions = new CosmosSerializationOptions()
                        {
                            IgnoreNullValues = true,
                            Indented = false,
                            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
                        },
                    };
                }

                return cosmosClientOptions;
            }
        }
    }
}
