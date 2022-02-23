// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LodeRunner.Core.Models
{
    /// <summary>
    ///   ClientStatus is primarily for conveying the current status, time of that status, and the LoadClient settings to consuming apps.
    /// </summary>
    public class ClientStatus : BaseEntityModel
    {
        private ClientStatusType status = ClientStatusType.Unknown;

        private string message = string.Empty;

        private int ttl = SystemConstants.ClientStatusExpirationTime;

        private LoadClient loadClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientStatus"/> class.
        /// </summary>
        public ClientStatus()
        {
            Debug.WriteLine("Client Status Constructor");
        }

        /// <summary>
        /// Gets the last updated.
        /// </summary>
        /// <value>
        /// The last updated.
        /// </value>
        [Required]
        public DateTime LastUpdated { get; private set; }

        /// <summary>
        /// Gets the last status change date time.
        /// </summary>
        /// <value>
        /// The duration of the status.
        /// </value>
        [Required]
        public DateTime LastStatusChange { get; private set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        [Required]
        public ClientStatusType Status
        {
            get
            {
                return this.status;
            }

            set
            {
                DateTime updatedDateTime = DateTime.UtcNow;

                this.LastUpdated = updatedDateTime;

                if (this.status != value)
                {
                    this.LastStatusChange = updatedDateTime;
                }

                this.status = value;
            }
        }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message
        {
            get
            {
                return this.message;
            }

            set
            {
                this.message = value;
                this.LastUpdated = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Gets or sets the load client.
        /// </summary>
        /// <value>
        /// The load client.
        /// </value>
        [Required]
        public LoadClient LoadClient
        {
            get
            {
                return this.loadClient;
            }

            set
            {
                this.loadClient = value;
                this.LastUpdated = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Gets or sets the Time to live in seconds.
        /// </summary>
        /// <value>
        /// The TTL.
        /// </value>
        [Required]
        public int Ttl
        {
            get
            {
                return this.ttl;
            }

            set
            {
                this.ttl = value;
                this.LastUpdated = DateTime.UtcNow;
            }
        }
    }
}
