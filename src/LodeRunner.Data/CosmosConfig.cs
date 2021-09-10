// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Cosmos;

namespace LodeRunner.Data
{
    /// <summary>
    /// Internal class for Cosmos config.
    /// </summary>
    internal sealed class CosmosConfig
    {
        /// <summary>
        /// The query request options.
        /// </summary>
        private QueryRequestOptions queryRequestOptions;

        /// <summary>
        /// The cosmos client options.
        /// </summary>
        private CosmosClientOptions cosmosClientOptions;

        /// <summary>
        /// Gets or sets the maximum rows.
        /// </summary>
        /// <value>
        /// The maximum rows.
        /// </value>
        public int MaxRows { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the timeout.
        /// </summary>
        /// <value>
        /// The timeout.
        /// </value>
        public int Timeout { get; set; } = 60;

        /// <summary>
        /// Gets or sets the retries.
        /// </summary>
        /// <value>
        /// The retries.
        /// </value>
        public int Retries { get; set; } = 10;

        /// <summary>
        /// Gets the CosmosDB query request options.
        /// </summary>
        /// <value>
        /// The query request options.
        /// </value>
        public QueryRequestOptions QueryRequestOptions
        {
            get
            {
                if (this.queryRequestOptions == default)
                {
                    this.queryRequestOptions = new QueryRequestOptions { MaxItemCount = this.MaxRows }; // , ConsistencyLevel = ConsistencyLevel.Session };
                }

                return this.queryRequestOptions;
            }
        }

        /// <summary>
        /// Gets the cosmos client options.
        /// default protocol is tcp, default connection mode is direct.
        /// </summary>
        /// <value>
        /// The cosmos client options.
        /// </value>
        public CosmosClientOptions CosmosClientOptions
        {
            get
            {
                if (this.cosmosClientOptions == default)
                {
                    this.cosmosClientOptions = new CosmosClientOptions { RequestTimeout = TimeSpan.FromSeconds(this.Timeout), MaxRetryAttemptsOnRateLimitedRequests = this.Retries, MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(this.Timeout) };
                }

                return this.cosmosClientOptions;
            }
        }
    }
}
