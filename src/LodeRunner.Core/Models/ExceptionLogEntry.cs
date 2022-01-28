// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net;

namespace LodeRunner.Core.Models
{
    /// <summary>
    /// Model used to log details about exceptions, including hints of known potential root causes.
    /// </summary>
    public class ExceptionLogEntry
    {
        /// <summary>
        /// Gets or sets the source of the exception.
        /// </summary>
        /// <value>
        /// The source of the exception.
        /// </value>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the HTTP status code of the exception.
        /// </summary>
        /// <value>
        /// The HTTP status code.
        /// </value>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Gets or sets exception message.
        /// </summary>
        /// <value>
        /// The exception message.
        /// </value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the debugging hint.
        /// </summary>
        /// <value>
        /// The debugging hint.
        /// </value>
        public string Hint { get; set; }
    }
}
