// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Ngsa.LodeRunner.DataAccessLayer.Model;

namespace Ngsa.DataAccessLayer.Model
{
    /// <summary>
    /// TestRun Model.
    /// </summary>
    public class TestRun : IBaseLoadEntityModel
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the partition key.
        /// </summary>
        /// <value>
        /// The partition key.
        /// </value>
        public string PartitionKey { get; set; }

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        public EntityType EntityType { get; set; } = EntityType.Unassigned;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the load test configuration.
        /// </summary>
        /// <value>
        /// The load test configuration.
        /// </value>
        public LoadTestConfig LoadTestConfig { get; set; }

        /// <summary>
        /// Gets or sets the load clients.
        /// </summary>
        /// <value>
        /// The load clients.
        /// </value>
        public List<LoadClient> LoadClients { get; set; }

        /// <summary>
        /// Gets or sets the created time.
        /// </summary>
        /// <value>
        /// The created time.
        /// </value>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        /// <value>
        /// The start time.
        /// </value>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the completed time.
        /// </summary>
        /// <value>
        /// The completed time.
        /// </value>
        public DateTime CompletedTime { get; set; }

        /// <summary>
        /// Gets or sets the client results.
        /// </summary>
        /// <value>
        /// The client results.
        /// </value>
        public List<LoadResult> ClientResults { get; set; }
    }
}
