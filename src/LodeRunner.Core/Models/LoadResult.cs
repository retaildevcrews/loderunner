// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using LodeRunner.Core.Models.Validators;

namespace LodeRunner.Core.Models
{
    /// <summary>
    /// LoadResult model.
    /// </summary>
    public class LoadResult : BaseEntityModel
    {
        /// <summary>
        /// Gets or sets the LoadClient.
        /// </summary>
        /// <value>
        /// LoadClient.
        /// </value>
        [Required]
        [ValidateList(ErrorMessage = "LoadClient must be set.")]
        [Description("LoadClient load test ran on.")]
        public virtual LoadClient LoadClient { get; set; }

        /// <summary>
        /// Gets or sets the schedule start time.
        /// </summary>
        /// <value>
        /// StartTime.
        /// </value>
        [Description("Schedule time to start the load test")]
        public virtual DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the time the load test on the LoadClient completed.
        /// </summary>
        /// <value>
        /// CompletedTime.
        /// </value>
        [Description("Completed load time test for a LoadClient")]
        public virtual DateTime CompletedTime { get; set; }

        /// <summary>
        /// Gets or sets the total number of requests.
        /// </summary>
        /// <value>
        /// TotalRequests.
        /// </value>
        [Description("Number of total requests")]
        public virtual int TotalRequests { get; set; }

        /// <summary>
        /// Gets or sets the number of successful requests.
        /// </summary>
        /// <value>
        /// SuccessfulRequests.
        /// </value>
        [Description("Number of successful requests")]
        public virtual int SuccessfulRequests { get; set; }

        /// <summary>
        /// Gets or sets the number of failed requests.
        /// </summary>
        /// <value>
        /// FailedRequests.
        /// </value>
        [Description("Number of failed requests")]
        public virtual int FailedRequests { get; set; }
    }
}
