// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ngsa.LodeRunner.DataAccessLayer.Model
{
    public class LoadClient
    {
        public string Id { get; set; }
        public string PartitionKey { get; set; }
        public string EntityType { get; set; }
        public string Name { get; set; }
        public List<string> Files { get; set; }
        public bool StrictJson { get; set; }
        public string BaseURL { get; set; }
        public bool VerboseErrors { get; set; }
        public bool Randomize { get; set; }
        public int Timeout { get; set; }
        public List<string> Server { get; set; }
        public string Tag { get; set; }
        public string Sleep { get; set; }
        public bool RunLoop { get; set; }
        public int Duration { get; set; }
        public int MaxErrors { get; set; }
        public int DelayStart { get; set; }
        public bool DryRun { get; set; }
    }
}
