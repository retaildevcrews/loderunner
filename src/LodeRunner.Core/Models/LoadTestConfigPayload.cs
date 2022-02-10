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
    /// LoadTestConfig Payload.
    /// </summary>
    [SwaggerSchemaFilter(typeof(LoadTestConfigPayloadSchemaFilter))]
    public class LoadTestConfigPayload : BasePayload
    {
        // Composite LoadTestConfig object to hold data
        private readonly LoadTestConfig loadTestConfig = new ();

        /// <summary>
        /// Gets or sets the files.
        /// </summary>
        /// <value>
        /// The files.
        /// </value>
        [Required]
        [ValidateList(ErrorMessage = "Files list cannot be null or empty.")]
        public List<string> Files { get => this.loadTestConfig.Files; set => this.SetField(this.loadTestConfig, value); }

        /// <summary>
        /// Gets or sets a value indicating whether [strict json].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [strict json]; otherwise, <c>false</c>.
        /// </value>
        public bool? StrictJson { get => this.loadTestConfig.StrictJson; set => this.SetField(this.loadTestConfig, value); }

        /// <summary>
        /// Gets or sets the base URL.
        /// </summary>
        /// <value>
        /// The base URL.
        /// </value>
        public string? BaseURL { get => this.loadTestConfig.BaseURL; set => this.SetField(this.loadTestConfig, value); }

        /// <summary>
        /// Gets or sets a value indicating whether [verbose errors].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [verbose errors]; otherwise, <c>false</c>.
        /// </value>
        public bool? VerboseErrors { get => this.loadTestConfig.VerboseErrors; set => this.SetField(this.loadTestConfig, value); }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="LoadTestConfig"/> is randomize.
        /// </summary>
        /// <value>
        ///   <c>true</c> if randomize; otherwise, <c>false</c>.
        /// </value>
        public bool? Randomize { get => this.loadTestConfig.Randomize; set => this.SetField(this.loadTestConfig, value); }

        /// <summary>
        /// Gets or sets the timeout.
        /// </summary>
        /// <value>
        /// The timeout.
        /// </value>
        public int? Timeout { get => this.loadTestConfig.Timeout; set => this.SetField(this.loadTestConfig, value); }

        /// <summary>
        /// Gets or sets the server.
        /// </summary>
        /// <value>
        /// The server.
        /// </value>
        [Required]
        [ValidateList(ErrorMessage = "Server list cannot be null or empty.")]
        public List<string> Server { get => this.loadTestConfig.Server; set => this.SetField(this.loadTestConfig, value); }

        /// <summary>
        /// Gets or sets the tag.
        /// </summary>
        /// <value>
        /// The tag.
        /// </value>
        public string? Tag { get => this.loadTestConfig.Tag; set => this.SetField(this.loadTestConfig, value); }

        /// <summary>
        /// Gets or sets the sleep.
        /// </summary>
        /// <value>
        /// The sleep.
        /// </value>
        public int? Sleep { get => this.loadTestConfig.Sleep; set => this.SetField(this.loadTestConfig, value); }

        /// <summary>
        /// Gets or sets a value indicating whether [run loop].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [run loop]; otherwise, <c>false</c>.
        /// </value>
        public bool? RunLoop { get => this.loadTestConfig.RunLoop; set => this.SetField(this.loadTestConfig, value); }

        /// <summary>
        /// Gets or sets the duration.
        /// </summary>
        /// <value>
        /// The duration.
        /// </value>
        public int? Duration { get => this.loadTestConfig.Duration; set => this.SetField(this.loadTestConfig, value); }

        /// <summary>
        /// Gets or sets the maximum errors.
        /// </summary>
        /// <value>
        /// The maximum errors.
        /// </value>
        public int? MaxErrors { get => this.loadTestConfig.MaxErrors; set => this.SetField(this.loadTestConfig, value); }

        /// <summary>
        /// Gets or sets the delay start.
        /// </summary>
        /// <value>
        /// The delay start.
        /// </value>
        [Range(0, 86400, ErrorMessage = "Can only be between 0 .. 86400")]
        [Description("--delay-start")]
        public int? DelayStart { get => this.loadTestConfig.DelayStart; set => this.SetField(this.loadTestConfig, value); }

        /// <summary>
        /// Gets or sets a value indicating whether [dry run].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [dry run]; otherwise, <c>false</c>.
        /// </value>
        public bool? DryRun { get => this.loadTestConfig.DryRun; set => this.SetField(this.loadTestConfig, value); }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get => this.loadTestConfig.Name; set => this.SetField(this.loadTestConfig, value); }
    }
}
