// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using LodeRunner.Core.Models.Validators;
using LodeRunner.Core.SchemaFilters;
using Swashbuckle.AspNetCore.Annotations;

namespace LodeRunner.Core.Models
{
    /// <summary>
    /// LoadTestConfig Model.
    /// </summary>
    [SwaggerSchemaFilter(typeof(LoadTestConfigSchemaFilter))]
    public class LoadTestConfig : BaseEntityModel
    {
        /// <summary>
        /// Gets or sets the files.
        /// </summary>
        /// <value>
        /// The files.
        /// </value>
        [Required]
        [ValidateList(ErrorMessage = "Files list cannot be null or empty.")]
        [Description("-f")]
        public virtual List<string> Files { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [strict json].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [strict json]; otherwise, <c>false</c>.
        /// </value>
        [Description("-j")]
        public virtual bool? StrictJson { get; set; }

        /// <summary>
        /// Gets or sets the base URL.
        /// </summary>
        /// <value>
        /// The base URL.
        /// </value>
        [Description("--base-url")]
        public virtual string? BaseURL { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [verbose errors].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [verbose errors]; otherwise, <c>false</c>.
        /// </value>
        [Description("--verbose-errors")]
        public virtual bool? VerboseErrors { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="LoadTestConfig"/> is randomize.
        /// </summary>
        /// <value>
        ///   <c>true</c> if randomize; otherwise, <c>false</c>.
        /// </value>
        [Description("--random")]
        public virtual bool? Randomize { get; set; }

        /// <summary>
        /// Gets or sets the timeout.
        /// </summary>
        /// <value>
        /// The timeout.
        /// </value>
        [Description("--timeout")]
        public virtual int? Timeout { get; set; }

        /// <summary>
        /// Gets or sets the server.
        /// </summary>
        /// <value>
        /// The server.
        /// </value>
        [Required]
        [ValidateList(ErrorMessage = "Server list cannot be null or empty.")]
        [Description("-s")]
        public virtual List<string> Server { get; set; }

        /// <summary>
        /// Gets or sets the tag.
        /// </summary>
        /// <value>
        /// The tag.
        /// </value>
        [Description("--tag")]
        public virtual string? Tag { get; set; }

        /// <summary>
        /// Gets or sets the sleep.
        /// </summary>
        /// <value>
        /// The sleep.
        /// </value>
        [Description("--sleep")]
        public virtual int? Sleep { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [run loop].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [run loop]; otherwise, <c>false</c>.
        /// </value>
        [Description("--run-loop")]
        public virtual bool? RunLoop { get; set; }

        /// <summary>
        /// Gets or sets the duration.
        /// </summary>
        /// <value>
        /// The duration.
        /// </value>
        [Description("--duration")]
        public virtual int? Duration { get; set; }

        /// <summary>
        /// Gets or sets the maximum errors.
        /// </summary>
        /// <value>
        /// The maximum errors.
        /// </value>
        [Description("--max-errors")]
        public virtual int? MaxErrors { get; set; }

        /// <summary>
        /// Gets or sets the delay start.
        /// </summary>
        /// <value>
        /// The delay start.
        /// </value>
        [Range(0, 86400, ErrorMessage = "Can only be between 0 .. 86400")]
        [Description("--delay-start")]
        public virtual int? DelayStart { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [dry run].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [dry run]; otherwise, <c>false</c>.
        /// </value>
        [Description("--dry-run")]
        public virtual bool? DryRun { get; set; }
    }
}
