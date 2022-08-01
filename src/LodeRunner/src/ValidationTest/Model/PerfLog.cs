﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace LodeRunner.Model
{
    /// <summary>
    /// Performance Log class.
    /// </summary>
    public class PerfLog
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PerfLog"/> class.
        /// </summary>
        /// <param name="validationErrors">list of validation errors.</param>
        public PerfLog(List<string> validationErrors)
        {
            this.Errors = validationErrors;
        }

        /// <summary>
        /// Gets the Type (defaults to request).
        /// </summary>
        public static string Type => "request";

        /// <summary>
        /// Gets or sets the DateTime.
        /// </summary>
        public DateTime Date { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// gets or sets the server URL.
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// Gets or sets the HTTP Status Code.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the failed flag.
        /// </summary>
        public bool Failed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the validated flag.
        /// </summary>
        public bool Validated { get; set; } = true;

        /// <summary>
        /// Gets or sets the B3 TraceId for distributed tracing.
        /// </summary>
        public string B3TraceId { get; set; }

        /// <summary>
        /// Gets or sets the Burst Load Feedback Value.
        /// </summary>
        public string BurstLoadFeedback { get; set; }

        /// <summary>
        /// Gets or sets the B3 SpanId  for distributed tracing.
        /// </summary>
        public string B3SpanId { get; set; }

        /// <summary>
        /// Gets the error count.
        /// </summary>
        public int ErrorCount => this.Errors == null ? 0 : this.Errors.Count;

        /// <summary>
        /// Gets or sets the duration.
        /// </summary>
        public double Duration { get; set; }

        /// <summary>
        /// Gets or sets the content length.
        /// </summary>
        public long ContentLength { get; set; }

        /// <summary>
        /// Gets or sets the Category.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the LoadClientId.
        /// </summary>
        public string LoadClientId { get; set; }

        /// <summary>
        /// Gets or sets the client status id.
        /// </summary>
        /// <value>
        /// The client status identifier.
        /// </value>
        public string ClientStatusId { get; set; }

        /// <summary>
        /// Gets or sets the test run id.
        /// </summary>
        /// <value>
        /// The test run identifier.
        /// </value>
        public string TestRunId { get; set; }

        /// <summary>
        /// Gets or sets the performance quartile.
        /// </summary>
        public int? Quartile { get; set; }

        /// <summary>
        /// Gets or sets the tag.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Gets or sets the request path.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets the validation errors.
        /// </summary>
        public List<string> Errors { get; }
    }
}
