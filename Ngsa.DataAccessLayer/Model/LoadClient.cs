// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Ngsa.DataAccessLayer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ngsa.LodeRunner.DataAccessLayer.Model
{
    public class LoadClient : IBaseLoadEntityModel
    {
        public string Id { get; set; }
        public string PartitionKey { get; set; }
        public EntityType EntityType { get; set; }
        public string Name { get; set; }
        public string Region { get; set; }
        public string Zone { get; set; }
        public bool Prometheus { get; set; }
        public string StartupArgs { get; set; }
        public DateTime StartTime { get; set; }
    }
}
