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
    /// ClientStatusRepository Settings.
    /// </summary>
    public class ClientStatusRepositorySettings : CosmosDBSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientStatusRepositorySettings"/> class.
        /// </summary>
        public ClientStatusRepositorySettings()
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
        /// Validates this instance.
        /// </summary>
        public override void Validate()
        {
            base.Validate();
            if (string.IsNullOrEmpty(this.CollectionName))
            {
                throw new ConfigurationErrorsException($"{this.GetType().Name}: CollectionName is invalid");
            }
        }
    }
}
