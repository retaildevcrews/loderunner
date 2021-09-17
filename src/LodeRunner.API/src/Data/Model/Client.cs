// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using LodeRunner.Core.Models;

namespace LodeRunner.API.Models
{
    public class Client
    {
        public Client(ClientStatus clientStatus)
        {
            LoadClientId = clientStatus.LoadClient.Id;
            Name = clientStatus.LoadClient.Name;
            Version = clientStatus.LoadClient.Version;
            Region = clientStatus.LoadClient.Region;
            Zone = clientStatus.LoadClient.Zone;
            Prometheus = clientStatus.LoadClient.Prometheus;
            StartupArgs = clientStatus.LoadClient.StartupArgs;
            StartTime = clientStatus.LoadClient.StartTime;
            ClientStatusId = clientStatus.Id;
            LastUpdated = clientStatus.LastUpdated;
            StatusDuration = clientStatus.StatusDuration;
            Status = clientStatus.Status;
            Message = clientStatus.Message;
        }

        public EntityType EntityType { get; } = EntityType.Client;
        public string LoadClientId { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Region { get; set; }
        public string Zone { get; set; }
        public bool Prometheus { get; set; }
        public string StartupArgs { get; set; }
        public DateTime StartTime { get; set; }
        public string ClientStatusId { get; set; }
        public DateTime LastUpdated { get; set; }
        public int StatusDuration { get; set; }
        public ClientStatusType Status { get; set; }
        public string Message { get; set; }
    }
}
