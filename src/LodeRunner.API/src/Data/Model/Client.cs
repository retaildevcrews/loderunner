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
        /// <example>Client</example>
        public EntityType EntityType { get; } = EntityType.Client;

        /// <summary>
        /// Gets or sets loadClientId.
        /// </summary>
        /// <example>7abcc308-14c4-43eb-b1ee-e351f4db2a08</example>
        public string LoadClientId { get; set; }

        /// <summary>
        /// Gets or sets client name.
        /// </summary>
        /// <example>Central - az - central - us - 2</example>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets client version.
        /// </summary>
        /// <example>0.3.0 - 717 - 1030</example>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets client region.
        /// </summary>
        /// <example>Central</example>
        public string Region { get; set; }

        /// <summary>
        /// Gets or sets client zone.
        /// </summary>
        /// <example>az-central-us</example>
        public string Zone { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether client has prometheus enabled.
        /// </summary>
        /// <example>false</example>
        public bool Prometheus { get; set; }

        /// <summary>
        /// Gets or sets client startup args.
        /// </summary>
        /// <example>--delay - start - 1--secrets - volume secrets</example>
        public string StartupArgs { get; set; }

        /// <summary>
        /// Gets or sets client start time.
        /// </summary>
        /// <example>2021-08-26T23:49:28.3828277Z</example>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets clientStatus ID.
        /// </summary>
        /// <example>c7975154-c88f-4188-8ca7-dba13ae7c9b2</example>
        public string ClientStatusId { get; set; }

        /// <summary>
        /// Gets or sets when client was last updated.
        /// </summary>
        /// <example>2021-08-26T23:49:56.6804331Z</example>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Gets or sets the last status change date time.
        /// </summary>
        /// <example>2021-08-26T23:49:29.5458555Z</example>
        public DateTime LastStatusChange { get; set; }

        /// <summary>
        /// Gets or sets client status.
        /// </summary>
        /// <example>Ready</example>
        public ClientStatusType Status { get; set; }

        /// <summary>
        /// Gets or sets client message.
        /// </summary>
        /// <example>Ready - test ready</example>
        public string Message { get; set; }
    }
}
