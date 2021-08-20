// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Configuration;
using Ngsa.LodeRunner.DataAccessLayer.Interfaces;

namespace Ngsa.LodeRunner.DataAccessLayer
{
    /// <summary>
    /// CosmosDB Settings.
    /// </summary>
    /// <seealso cref="Ngsa.LodeRunner.DataAccessLayer.Interfaces.ICosmosDBSettings" />
    public class CosmosDBSettings : ICosmosDBSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDBSettings"/> class.
        /// </summary>
        public CosmosDBSettings()
        {
        }

        /// <summary>
        /// Gets or sets the retries.
        /// </summary>
        /// <value>
        /// The retries.
        /// </value>
        public int Retries { get; set; }

        /// <summary>
        /// Gets or sets the timeout.
        /// </summary>
        /// <value>
        /// The timeout.
        /// </value>
        public int Timeout { get; set; }

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
        /// Validates this instance.
        /// </summary>
        public virtual void Validate()
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
        }
    }
}
