// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Ngsa.LodeRunner.DataAccessLayer
{
    /// <summary>
    /// ChangeFeedProcessorRepositorySettings Settings.
    /// </summary>
    public class ChangeFeedProcessorRepositorySettings : CosmosDBSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeFeedProcessorRepositorySettings"/> class.
        /// </summary>
        public ChangeFeedProcessorRepositorySettings()
        {
        }

        /// <summary>
        /// Gets or sets the name of the collection.
        /// </summary>
        /// <value>
        /// The name of the collection.
        /// </value>
        public string CollectionName { get; set; }

        /// <summary>
        /// Gets or sets the cosmos lease.
        /// </summary>
        /// <value>
        /// The cosmos lease.
        /// </value>
        public string CosmosLease { get; set;  }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        public override void Validate()
        {
            base.Validate();
            if (string.IsNullOrEmpty(this.CollectionName))
            {
                throw new ConfigurationErrorsException($"{this.GetType().Name}: CollectionName is invalid");
            }

            if (string.IsNullOrEmpty(this.CosmosLease))
            {
                throw new ConfigurationErrorsException($"{this.GetType().Name}: CosmosLease is invalid");
            }
        }
    }
}
