// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
namespace Ngsa.LodeRunner.Events
{
    public class ClientStatusEventArgs : EventArgs
    {
        //public string PartitionKey { get; } = Guid.NewGuid().ToString();
        //public string EntityType { get; set; }
        //public string Id { get; set; }
        //public string Name { get; set; }
        public DateTime LastUpdated { get; set; }
        //public int StateDuration { get; set; }
        public string Status { get; set; } //TODO Convert status to enum when merging with dal
        public string Message { get; set; }
        //public LoadClient LoadClient { get; set; }



        public ClientStatusEventArgs(string status, string message)
        {
            //PartitionKey = partitionKey;
            //EntityType = entityType;
            //Id = id;
            //Name = name;
            LastUpdated = DateTime.UtcNow;
            //StateDuration = stateDuration;
            Status = status;
            Message = message;
        }
    }
}
