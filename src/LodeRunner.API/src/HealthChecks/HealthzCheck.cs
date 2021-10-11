// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LodeRunner.API.Models
{
    /// <summary>
    /// Health Check that supports dotnet IHeathCheck.
    /// </summary>
    public class HealthzCheck
    {
        /// <summary>
        /// Health chec timeout message.
        /// </summary>
        public const string TimeoutMessage = "Request exceeded expected duration";

        /// <summary>
        /// Gets or sets health check status.
        /// </summary>
        public HealthStatus Status { get; set; }

        /// <summary>
        /// Gets or sets component ID.
        /// </summary>
        public string ComponentId { get; set; }

        /// <summary>
        /// Gets or sets component type.
        /// </summary>
        public string ComponentType { get; set; }

        /// <summary>
        /// Gets or sets duration.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Gets or sets target duration.
        /// </summary>
        public TimeSpan TargetDuration { get; set; }

        /// <summary>
        /// Gets or sets time.
        /// </summary>
        public string Time { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

        /// <summary>
        /// Gets or sets the endpoint.
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message { get; set; }
    }
}
