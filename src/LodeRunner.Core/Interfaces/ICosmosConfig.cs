// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace LodeRunner.Core.Interfaces
{
    /// <summary>
    /// Configuration for Cosmos.
    /// </summary>
    public interface ICosmosConfig
    {
        /// <summary>
        /// Gets the secrets volume.
        /// </summary>
        /// <value>
        /// The secrets volume.
        /// </value>
        string SecretsVolume { get; }

        /// <summary>
        /// Gets the name of the cosmos.
        /// </summary>
        /// <value>
        /// The name of the cosmos.
        /// </value>
        string CosmosName { get; }

        /// <summary>
        /// Gets the secrets.
        /// </summary>
        /// <value>
        /// The secrets.
        /// </value>
        ISecrets Secrets { get;  }

        /// <summary>
        /// Gets the retries.
        /// </summary>
        /// <value>
        /// The retries.
        /// </value>
        int Retries { get;  }

        /// <summary>Gets the cosmos max retry wait time for cosmos requests in seconds.</summary>
        /// <value>Time in seconds.</value>
        int CosmosTimeout { get; }
    }
}
