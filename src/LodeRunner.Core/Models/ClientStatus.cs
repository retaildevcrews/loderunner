// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace LodeRunner.Core.Models
{
    /// <summary>
    ///   ClientStatus is primarily for conveying the current status, time of that status, and the LoadClient settings to consuming apps.
    /// </summary>
    public class ClientStatus : BaseEntityModel
    {
        /// <summary>
        /// Gets or sets the last updated.
        /// </summary>
        /// <value>
        /// The last updated.
        /// </value>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Gets or sets the duration of the status.
        /// </summary>
        /// <value>
        /// The duration of the status.
        /// </value>
        public int StatusDuration { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public ClientStatusType Status { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the load client.
        /// </summary>
        /// <value>
        /// The load client.
        /// </value>
        public LoadClient LoadClient { get; set; }

        /// <summary>
        /// Gets or sets the Time to live in seconds.
        /// </summary>
        /// <value>
        /// The TTL.
        /// </value>
        public int Ttl { get; set; } = SystemConstants.ClientStatusExpirationTime;
    }
}
