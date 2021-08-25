// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Azure.Cosmos;
using Ngsa.LodeRunner.DataAccessLayer.Model;

namespace Ngsa.LodeRunner.DataAccessLayer.Interfaces
{
    /// <summary>
    /// ChangeFeedProcessorRepository Interface.
    /// </summary>
    public interface IChangeFeedProcessorRepository : ICosmosDBRepository<ClientStatus>
    {
        /// <summary>
        /// Gets the source container.
        /// </summary>
        /// <value>
        /// The source container.
        /// </value>
        Container SourceContainer { get; }

        /// <summary>
        /// Gets the lease container.
        /// </summary>
        /// <value>
        /// The lease container.
        /// </value>
        Container LeaseContainer { get; }
    }
}
