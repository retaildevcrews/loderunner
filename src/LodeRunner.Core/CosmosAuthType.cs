// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace LodeRunner.Core
{
    /// <summary>
    /// CosmosAuthTYpe.
    /// </summary>
    public enum CosmosAuthType
    {
        /// <summary>
        /// Indicates to use secret key for Cosmos.
        /// </summary>
        SecretKey,

        /// <summary>
        /// Indicates to use Managed Identity.
        /// </summary>
        ManagedIdentity,
    }
}
