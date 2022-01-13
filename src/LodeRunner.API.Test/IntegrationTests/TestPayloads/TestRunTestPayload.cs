// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using LodeRunner.Core.Models;

namespace LodeRunner.API.Test.IntegrationTests.Payloads
{
#pragma warning disable SA1600 // Elements should be documented
    public class TestRunTestPayload
    {
        public string Name { get; set; } = "Sample - TestRun - JOHEE001";

        public DateTime CreatedTime { get; set; } = new DateTime(2022, 1, 7, 0, 0, 0);

        public DateTime StartTime { get; set; } = new DateTime(2022, 1, 7, 0, 30, 0);

        public LoadTestConfig LoadTestConfig { get; set; } = new LoadTestConfig()
        {
            Id = "7abcc308-14c4-43eb-b1ee-e351f4db2a08",
            Name = "Sample - LoadTestConfig",
            Files = new List<string>() { "baseline.json", "benchmark.json" },
            StrictJson = true,
            BaseURL = "Sample BaseURL",
            VerboseErrors = true,
            Randomize = true,
            Timeout = 10,
            Server = new List<string>() { "www.yourprimaryserver.com", "www.yoursecondaryserver.com" },
            Tag = "Sample Tag",
            Sleep = 5,
            RunLoop = true,
            Duration = 60,
            MaxErrors = 10,
            DelayStart = 5,
            DryRun = false,
        };

        public List<LoadClient> LoadClients { get; set; } = new List<LoadClient>
        {
            new LoadClient()
            {
                Id = "13eff199-38ee-46ee-92df-d064025db4e7",
                Version = "1.0.1",
                Region = "Central",
                Zone = "central-az-1",
                Prometheus = true,
                StartupArgs = "--mode Client --region Central --zone central-az-1 --prometheus true",
                StartTime = new DateTime(2022, 1, 1, 2, 3, 4),
            },
        };
    }
#pragma warning disable SA1600 // Elements should be documented
}
