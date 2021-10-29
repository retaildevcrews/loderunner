// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using LodeRunner.Core.Models;

namespace LodeRunner.API.Models
{
    /// <summary>
    /// Model of client.
    /// </summary>
    public class Client
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// </summary>
        /// <param name="clientStatus">ClientStatus.</param>
        public Client(ClientStatus clientStatus)
        {
            this.LoadClientId = clientStatus.LoadClient?.Id;
            this.Name = clientStatus.LoadClient?.Name;
            this.Version = clientStatus.LoadClient?.Version;
            this.Region = clientStatus.LoadClient?.Region;
            this.Zone = clientStatus.LoadClient?.Zone;
            this.Prometheus = clientStatus.LoadClient != null && clientStatus.LoadClient.Prometheus;
            this.StartupArgs = clientStatus.LoadClient?.StartupArgs;
            this.StartTime = clientStatus.LoadClient != null ? clientStatus.LoadClient.StartTime : DateTime.MinValue;
            this.ClientStatusId = clientStatus.Id;
            this.LastUpdated = clientStatus.LastUpdated;
            this.LastStatusChange = clientStatus.LastStatusChange;
            this.Status = clientStatus.Status;
            this.Message = clientStatus.Message;
        }

        /// <summary>
        /// Gets entityType of client.
        /// </summary>
        public EntityType EntityType { get; } = EntityType.Client;

        /// <summary>
        /// Gets or sets loadClientId.
        /// </summary>
        public string LoadClientId { get; set; }

        /// <summary>
        /// Gets or sets client name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets client version.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets client region.
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Gets or sets client zone.
        /// </summary>
        public string Zone { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether client has prometheus enabled.
        /// </summary>
        public bool Prometheus { get; set; }

        /// <summary>
        /// Gets or sets client startup args.
        /// </summary>
        public string StartupArgs { get; set; }

        /// <summary>
        /// Gets or sets client start time.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets clientStatus ID.
        /// </summary>
        public string ClientStatusId { get; set; }

        /// <summary>
        /// Gets or sets when client was last updated.
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Gets or sets the last status change date time.
        /// </summary>
        public DateTime LastStatusChange { get; set; }

        /// <summary>
        /// Gets or sets client status.
        /// </summary>
        public ClientStatusType Status { get; set; }

        /// <summary>
        /// Gets or sets client message.
        /// </summary>
        public string Message { get; set; }
    }
}
