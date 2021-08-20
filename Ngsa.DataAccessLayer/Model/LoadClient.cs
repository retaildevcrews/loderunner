// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Ngsa.DataAccessLayer.Model;

namespace Ngsa.LodeRunner.DataAccessLayer.Model
{
    /// <summary>
    /// LoadClient Model.
    /// </summary>
    /// <seealso cref="Ngsa.LodeRunner.DataAccessLayer.Model.IBaseLoadEntityModel" />
    public class LoadClient : IBaseLoadEntityModel
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
        public EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the region.
        /// </summary>
        /// <value>
        /// The region.
        /// </value>
        public string Region { get; set; }

        /// <summary>
        /// Gets or sets the zone.
        /// </summary>
        /// <value>
        /// The zone.
        /// </value>
        public string Zone { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="LoadClient"/> is prometheus.
        /// </summary>
        /// <value>
        ///   <c>true</c> if prometheus; otherwise, <c>false</c>.
        /// </value>
        public bool Prometheus { get; set; }

        /// <summary>
        /// Gets or sets the startup arguments.
        /// </summary>
        /// <value>
        /// The startup arguments.
        /// </value>
        public string StartupArgs { get; set; }

        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        /// <value>
        /// The start time.
        /// </value>
        public DateTime StartTime { get; set; }
    }
}
