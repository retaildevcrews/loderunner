// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LodeRunner.API.Models
{
    /// <summary>
    /// Health Check that supports IETF json.
    /// </summary>
    public class IetfCheck
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IetfCheck"/> class.
        /// </summary>
        public IetfCheck()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IetfCheck"/> class.
        /// Create an IetfCheck from a HealthzCheck.
        /// </summary>
        /// <param name="hzCheck">HealthzCheck.</param>
        public IetfCheck(HealthzCheck hzCheck)
        {
            if (hzCheck == null)
            {
                throw new ArgumentNullException(nameof(hzCheck));
            }

            this.Status = ToIetfStatus(hzCheck.Status);
            this.ComponentId = hzCheck.ComponentId;
            this.ComponentType = hzCheck.ComponentType;
            this.ObservedValue = Math.Round(hzCheck.Duration.TotalMilliseconds, 2);
            this.TargetValue = Math.Round(hzCheck.TargetDuration.TotalMilliseconds, 0);
            this.ObservedUnit = "ms";
            this.Time = hzCheck.Time;
            this.Message = hzCheck.Message;

            if (hzCheck.Status != HealthStatus.Healthy && !string.IsNullOrEmpty(hzCheck.Endpoint))
            {
                this.AffectedEndpoints = new List<string> { hzCheck.Endpoint };
            }
        }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the component ID.
        /// </summary>
        public string ComponentId { get; set; }

        /// <summary>
        /// Gets or sets the component type.
        /// </summary>
        public string ComponentType { get; set; }

        /// <summary>
        /// Gets or sets the observed unit.
        /// </summary>
        public string ObservedUnit { get; set; }

        /// <summary>
        /// Gets or sets the observed value.
        /// </summary>
        public double ObservedValue { get; set; }

        /// <summary>
        /// Gets or sets the target value.
        /// </summary>
        public double TargetValue { get; set; }

        /// <summary>
        /// Gets or sets the time.
        /// </summary>
        public string Time { get; set; }

        /// <summary>
        /// Gets the affected endpoints.
        /// </summary>
        public List<string> AffectedEndpoints { get; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Convert the dotnet HealthStatus to the IETF Status.
        /// </summary>
        /// <param name="status">HealthStatus (dotnet).</param>
        /// <returns>string.</returns>
        public static string ToIetfStatus(HealthStatus status)
        {
            return status switch
            {
                HealthStatus.Healthy => "pass",
                HealthStatus.Degraded => "warn",
                _ => "fail"
            };
        }
    }
}
