// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Configuration;
using Ngsa.LodeRunner.DataAccessLayer.Interfaces;

namespace Ngsa.LodeRunner.DataAccessLayer
{
    public class CosmosDBSettings : ICosmosDBSettings
    {
        public CosmosDBSettings()
        {
        }

        public int Retries { get; set; }

        public int Timeout { get; set; }

        public string Uri { get; set; }

        public string Key { get; set; }

        public string DatabaseName { get; set; }

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
