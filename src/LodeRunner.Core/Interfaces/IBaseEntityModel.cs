// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using LodeRunner.Core.Models;

namespace LodeRunner.Core.Interfaces
{
    /// <summary>
    /// Base LoadEntity Model Interface.
    /// </summary>
    public interface IBaseEntityModel
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        string Id { get; set; }

        /// <summary>
        /// Gets the partition key.
        /// </summary>
        /// <value>
        /// The partition key.
        /// </value>
        string PartitionKey { get; }

        /// <summary>
        /// Gets  the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        EntityType EntityType { get; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; set; }
    }
}
