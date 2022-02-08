// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace LodeRunner.Core.Events
{
    /// <summary>
    /// Represents the LoadResultEventArgs and contains the main functionality of the class.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class LoadResultEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadResultEventArgs"/> class.
        /// </summary>
        /// <param name="startTime">Start time of the TestRun.</param>
        /// <param name="completedTime">Time TestRun completed.</param>
        /// <param name="testRunId">Id of the TestRun.</param>
        /// <param name="total">Number of total requests.</param>
        /// <param name="failed">Number of failed requests.</param>
        /// <param name="errorMessage">Error message (if exception occurred).</param>
        public LoadResultEventArgs(DateTime startTime, DateTime completedTime, string testRunId, int total, int failed, string errorMessage = "")
        {
            this.StartTime = startTime;
            this.CompletedTime = completedTime;
            this.TestRunId = testRunId;
            this.TotalRequests = total;
            this.FailedRequests = failed;
            this.SuccessfulRequests = total - failed;
            this.ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Gets or sets the time the load test on the LoadClient started.
        /// </summary>
        /// <value>
        /// StartTime.
        /// </value>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the time the load test on the LoadClient completed.
        /// </summary>
        /// <value>
        /// CompletedTime.
        /// </value>
        public DateTime CompletedTime { get; set; }

        /// <summary>
        /// Gets or sets the TestRun id that was executed.
        /// </summary>
        /// <value>
        /// TestRunId.
        /// </value>
        public string TestRunId { get; set; }

        /// <summary>
        /// Gets or sets the total number of requests.
        /// </summary>
        /// <value>
        /// TotalRequests.
        /// </value>
        public int TotalRequests { get; set; }

        /// <summary>
        /// Gets or sets the number of successful requests.
        /// </summary>
        /// <value>
        /// SuccessfulRequests.
        /// </value>
        public int SuccessfulRequests { get; set; }

        /// <summary>
        /// Gets or sets the number of failed requests.
        /// </summary>
        /// <value>
        /// FailedRequests.
        /// </value>
        public int FailedRequests { get; set; }

        /// <summary>
        /// Gets or sets the error message (if exception occurred).
        /// </summary>
        /// <value>
        /// ErrorMessage.
        /// </value>
        public string ErrorMessage { get; set; }
    }
}
