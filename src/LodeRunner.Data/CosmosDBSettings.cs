// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Configuration;
using LodeRunner.Core.Interfaces;
using LodeRunner.Data.Interfaces;

namespace LodeRunner.Data
{
    /// <summary>
    /// CosmosDB Settings.
    /// </summary>
    /// <seealso cref="LodeRunner.Data.Interfaces.ICosmosDBSettings" />
    public sealed class CosmosDBSettings : ISettingsValidator, ICosmosDBSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDBSettings"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public CosmosDBSettings(ICosmosConfig config)
        {
            this.CollectionName = config.Secrets.CosmosCollection;
            this.Retries = config.Retries;
            this.Timeout = config.CosmosTimeout;
            this.Uri = config.Secrets.CosmosServer;
            this.Key = config.Secrets.CosmosKey;
            this.DatabaseName = config.Secrets.CosmosDatabase;
        }

        /// <summary>
        /// Gets or sets the cosmos maximum retries.
        /// </summary>
        /// <value>
        /// The retries.
        /// </value>
        public int Retries { get; set; } = 10;

        /// <summary>
        /// Gets or sets the cosmos timeout.
        /// </summary>
        /// <value>
        /// The timeout.
        /// </value>
        public int Timeout { get; set; } = 60;

        /// <summary>
        /// Gets or sets the URI.
        /// </summary>
        /// <value>
        /// The URI.
        /// </value>
        public string Uri { get; set; }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the name of the database.
        /// </summary>
        /// <value>
        /// The name of the database.
        /// </value>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the name of the collection.
        /// </summary>
        /// <value>
        /// The name of the collection.
        /// </value>
        public string CollectionName { get; set; }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(this.Uri))
            {
                throw new ConfigurationErrorsException($"{this.GetType().Name}: Uri is invalid");
            }

            if (string.IsNullOrWhiteSpace(this.Key))
            {
                throw new ConfigurationErrorsException($"{this.GetType().Name}: Key is invalid");
            }

            if (string.IsNullOrWhiteSpace(this.DatabaseName))
            {
                throw new ConfigurationErrorsException($"{this.GetType().Name}: DatabaseName is invalid");
            }

            if (string.IsNullOrEmpty(this.CollectionName))
            {
                throw new ConfigurationErrorsException($"{this.GetType().Name}: CollectionName is invalid");
            }
        }
    }
}
