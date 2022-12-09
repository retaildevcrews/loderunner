// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using LodeRunner.Core;

namespace LodeRunner.Data.Interfaces
{
    /// <summary>
    /// CosmosDB Settings Interface.
    /// </summary>
    public interface ICosmosDBSettings : ISettingsValidator
    {
        /// <summary>
        /// Gets or sets cosmosAuthType.
        /// </summary>
        /// <value>
        /// The CosmosAuthType.
        /// </value>
        public CosmosAuthType CosmosAuthType { get; set; }

        /// <summary>
        /// Gets the retries.
        /// </summary>
        /// <value>
        /// The retries.
        /// </value>
        int Retries { get; }

        /// <summary>
        /// Gets the timeout.
        /// </summary>
        /// <value>
        /// The timeout.
        /// </value>
        int Timeout { get; }

        /// <summary>
        /// Gets the URI.
        /// </summary>
        /// <value>
        /// The URI.
        /// </value>
        string Uri { get; }

        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        string Key { get; }

        /// <summary>
        /// Gets the name of the database.
        /// </summary>
        /// <value>
        /// The name of the database.
        /// </value>
        string DatabaseName { get; }

        /// <summary>
        /// Gets or sets the name of the collection.
        /// </summary>
        /// <value>
        /// The name of the collection.
        /// </value>
        string CollectionName { get; set; }

        /// <summary>
        /// Gets or sets the database cosmos connection check interval.
        /// </summary>
        /// <value>
        /// The database cosmos connection check interval.
        /// </value>
        int CosmosDbConnectionCheckInterval { get; set; }

        /// <summary>
        /// Gets the cosmos database connection retry limit.
        /// </summary>
        /// <value>
        /// The cosmos database connection retry limit.
        /// </value>
        int CosmosDbConnectionCheckRetryLimit { get; }
    }
}
