// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LodeRunner.Core.SchemaFilters;
using Swashbuckle.AspNetCore.Annotations;

namespace LodeRunner.Core.Models
{
    /// <summary>
    /// TestRun Model.
    /// </summary>
    [SwaggerSchemaFilter(typeof(TestRunSchemaFilter))]
    public class TestRun : BaseEntityModel
    {
        /// <summary>
        /// Gets or sets the load test configuration.
        /// </summary>
        /// <value>
        /// The load test configuration.
        /// </value>
        [Required]
        public LoadTestConfig LoadTestConfig { get; set; }

        /// <summary>
        /// Gets or sets the load clients.
        /// </summary>
        /// <value>
        /// The load clients.
        /// </value>
        [Required]
        public List<LoadClient> LoadClients { get; set; }

        /// <summary>
        /// Gets or sets the created time.
        /// </summary>
        /// <value>
        /// The created time.
        /// </value>
        [Required]
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        /// <value>
        /// The start time.
        /// </value>
        [Required]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets completed time for this TestRun.
        /// If null, this property won't exist on serialization.
        /// </summary>
        /// <value>
        /// CompletedTime.
        /// </value>
        public DateTime? CompletedTime { get; set; } = null;

        /// <summary>
        /// Gets or sets the client results.
        /// </summary>
        /// <value>
        /// The client results.
        /// </value>
        public List<LoadResult> ClientResults { get; set; } = new ();
    }
}
