// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using LodeRunner.Core.Models;

namespace LodeRunner.Core.Events
{
    /// <summary>
    /// Represents the ClientStatusEventArgs and contains the main functionality of the class.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class ClientStatusEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientStatusEventArgs"/> class.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <param name="message">The message.</param>
        public ClientStatusEventArgs(ClientStatusType status, string message)
        {
            this.LastUpdated = DateTime.UtcNow;
            this.Status = status;
            this.Message = message;
        }

        /// <summary>
        /// Gets or sets the last updated.
        /// </summary>
        /// <value>
        /// The last updated.
        /// </value>
        public DateTime LastUpdated { get; set; }

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
    }
}
