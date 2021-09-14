// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using LodeRunner.API.Models.Enum;

namespace LodeRunner.API.Models
{
    public class ClientStatus
    {
        public string PartitionKey { get; set; }
        public EntityType EntityType { get; set; }
        public string Id { get; set; }
        public DateTime LastUpdated { get; set; }
        public int StatusDuration { get; set; }
        public Status Status { get; set; }
        public string Message { get; set; }
        public LoadClient LoadClient { get; set; }

        /// <summary>
        /// Compute the partition key based on the EntityType
        /// </summary>
        /// <param name="entityType">EntityType</param>
        /// <returns>the partition key</returns>
        public static string ComputePartitionKey()
        {
            return EntityType.ClientStatus.ToString();
        }
    }
}
