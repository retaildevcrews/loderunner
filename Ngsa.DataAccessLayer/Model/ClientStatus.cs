// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ngsa.LodeRunner.DataAccessLayer.Model
{
    public class ClientStatus
    {
        public string Id { get; set; }
        public string PartitionKey { get; set; }
        public EntityType EntityType { get; set; }
        public DateTime LastUpdated { get; set; }
        public int StateDuration { get; set; }
        public ClientStatusType Status { get; set; }
        public string Message { get; set; }
        public LoadClient LoadClient { get; set; }
    }
}
