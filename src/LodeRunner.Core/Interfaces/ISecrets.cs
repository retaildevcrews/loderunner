// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace LodeRunner.Core.Interfaces
{
    /// <summary>
    /// Represents the Secrets Interface.
    /// </summary>
    public interface ISecrets
    {
        /// <summary>
        /// Gets the volume.
        /// </summary>
        /// <value>
        /// The volume.
        /// </value>
        string Volume { get; }

        /// <summary>
        /// Gets the cosmos server.
        /// </summary>
        /// <value>
        /// The cosmos server.
        /// </value>
        string CosmosServer { get; }

        /// <summary>
        /// Gets the cosmos key.
        /// </summary>
        /// <value>
        /// The cosmos key.
        /// </value>
        string CosmosKey { get; }

        /// <summary>
        /// Gets the cosmos database.
        /// </summary>
        /// <value>
        /// The cosmos database.
        /// </value>
        string CosmosDatabase { get;  }

        /// <summary>
        /// Gets the cosmos collection.
        /// </summary>
        /// <value>
        /// The cosmos collection.
        /// </value>
        string CosmosCollection { get; }
    }
}
