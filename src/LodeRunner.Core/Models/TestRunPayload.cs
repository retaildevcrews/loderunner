// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using LodeRunner.Core.Models.Validators;
using LodeRunner.Core.SchemaFilters;
using Swashbuckle.AspNetCore.Annotations;

namespace LodeRunner.Core.Models
{
    /// <summary>
    /// TestRun Payload.
    /// </summary>
    [SwaggerSchemaFilter(typeof(TestRunPayloadSchemaFilter))]
    public class TestRunPayload : BasePayload
    {
        /// <summary>
        /// Gets or sets the LoadTestConfig.
        /// </summary>
        /// <value>
        /// The LoadTestConfig.
        /// </value>
        public LoadTestConfig LoadTestConfig { get; set; }

        /// <summary>
        /// Gets or sets the LoadClients.
        /// </summary>
        /// <value>
        /// The LoadClients.
        /// </value>
        [ValidateList(ErrorMessage = "LoadClients list cannot be null or empty.")]
        public List<LoadClient> LoadClients { get; set; }

        /// <summary>
        /// Gets or sets the CreatedTime.
        /// It refers to the creation time of this TestRun.
        /// </summary>
        /// <value>
        /// The CreatedTime.
        /// </value>
        public DateTime? CreatedTime { get; set; }

        /// <summary>
        /// Gets or sets the StartTime.
        /// It refers to the time the TestRun is scheduled to execute.
        /// </summary>
        /// <value>
        /// The StartTime.
        /// </value>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// Gets or sets the CompletedTime.
        /// It refers to the time the TestRun is completed.
        /// </summary>
        /// <value>
        /// The CompletedTime.
        /// </value>
        public DateTime? CompletedTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Hard Stop the Test Run.
        /// </summary>
        public bool HardStop { get; set; } = false;

        /// <summary>
        /// Gets or sets hard stop time for this TestRun.
        /// If null, this property won't exist on serialization.
        /// </summary>
        /// <value>
        /// HardStopTime.
        /// </value>
        public DateTime? HardStopTime { get; set; } = null;
    }
}
