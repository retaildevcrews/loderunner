// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Ngsa.LodeRunner.DataAccessLayer
{
    public class ClientStatusRepositorySettings : CosmosDBSettings
    {
        public ClientStatusRepositorySettings()
        {
        }

        public string CollectionName { get; set; }

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
