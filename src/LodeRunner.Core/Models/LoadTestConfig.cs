// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace LodeRunner.Core.Models
{
    /// <summary>
    /// LoadRestConfig Model.
    /// </summary>
    public class LoadTestConfig : BaseEntityModel
    {
        /// <summary>
        /// Gets or sets the files.
        /// </summary>
        /// <value>
        /// The files.
        /// </value>
        public List<string> Files { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [strict json].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [strict json]; otherwise, <c>false</c>.
        /// </value>
        public bool StrictJson { get; set; }

        /// <summary>
        /// Gets or sets the base URL.
        /// </summary>
        /// <value>
        /// The base URL.
        /// </value>
        public string BaseURL { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [verbose errors].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [verbose errors]; otherwise, <c>false</c>.
        /// </value>
        public bool VerboseErrors { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="LoadTestConfig"/> is randomize.
        /// </summary>
        /// <value>
        ///   <c>true</c> if randomize; otherwise, <c>false</c>.
        /// </value>
        public bool Randomize { get; set; }

        /// <summary>
        /// Gets or sets the timeout.
        /// </summary>
        /// <value>
        /// The timeout.
        /// </value>
        public int Timeout { get; set; }

        /// <summary>
        /// Gets or sets the server.
        /// </summary>
        /// <value>
        /// The server.
        /// </value>
        public List<string> Server { get; set; }

        /// <summary>
        /// Gets or sets the tag.
        /// </summary>
        /// <value>
        /// The tag.
        /// </value>
        public string Tag { get; set; }

        /// <summary>
        /// Gets or sets the sleep.
        /// </summary>
        /// <value>
        /// The sleep.
        /// </value>
        public string Sleep { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [run loop].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [run loop]; otherwise, <c>false</c>.
        /// </value>
        public bool RunLoop { get; set; }

        /// <summary>
        /// Gets or sets the duration.
        /// </summary>
        /// <value>
        /// The duration.
        /// </value>
        public int Duration { get; set; }

        /// <summary>
        /// Gets or sets the maximum errors.
        /// </summary>
        /// <value>
        /// The maximum errors.
        /// </value>
        public int MaxErrors { get; set; }

        /// <summary>
        /// Gets or sets the delay start.
        /// </summary>
        /// <value>
        /// The delay start.
        /// </value>
        public int DelayStart { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [dry run].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [dry run]; otherwise, <c>false</c>.
        /// </value>
        public bool DryRun { get; set; }
    }
}
