// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
        // Composite TestRun object to hold data
        private readonly TestRun testRun = new ();

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get => this.testRun.Name; set => this.SetField(this.testRun, value); }

        /// <summary>
        /// Gets or sets the LoadTestConfig.
        /// </summary>
        /// <value>
        /// The LoadTestConfig.
        /// </value>
        public LoadTestConfig LoadTestConfig { get => this.testRun.LoadTestConfig; set => this.SetField(this.testRun, value); }

        /// <summary>
        /// Gets or sets the LoadClients.
        /// </summary>
        /// <value>
        /// The LoadClients.
        /// </value>
        public List<LoadClient> LoadClients { get => this.testRun.LoadClients; set => this.SetField(this.testRun, value); }

        /// <summary>
        /// Gets or sets the CreatedTime.
        /// It refers to the creation time of this TestRun.
        /// </summary>
        /// <value>
        /// The CreatedTime.
        /// </value>
        public DateTime CreatedTime { get => this.testRun.CreatedTime; set => this.SetField(this.testRun, value); }

        /// <summary>
        /// Gets or sets the StartTime.
        /// It refers to the time the TestRun is scheduled to execute.
        /// </summary>
        /// <value>
        /// The StartTime.
        /// </value>
        public DateTime StartTime { get => this.testRun.StartTime; set => this.SetField(this.testRun, value); }

        /// <summary>
        /// Gets or sets the CompletedTime.
        /// It refers to the time the TestRun is completed.
        /// </summary>
        /// <value>
        /// The CompletedTime.
        /// </value>
        public DateTime? CompletedTime { get => this.testRun.CompletedTime; set => this.SetField(this.testRun, value); }
    }
}
