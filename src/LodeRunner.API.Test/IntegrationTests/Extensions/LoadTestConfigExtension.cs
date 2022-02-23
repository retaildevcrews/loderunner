// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using LodeRunner.Core.Models;

namespace LodeRunner.API.Test.IntegrationTests
{
    /// <summary>
    /// LoadTestConfig Extension.
    /// </summary>
    public static class LoadTestConfigExtension
    {
        /// <summary>
        /// Sets mock LoadTestConfig data.
        /// </summary>
        /// <param name="loadTestConfig">LoadTestConfig.</param>
        /// <param name="name">LoadTestConfig name.</param>
        public static void SetMockData(this LoadTestConfig loadTestConfig, string name)
        {
            loadTestConfig.Name = name;
            loadTestConfig.Files = new List<string>() { "baseline.json", "benchmark.json" };
            loadTestConfig.StrictJson = true;
            loadTestConfig.BaseURL = "SampleBaseURL";
            loadTestConfig.VerboseErrors = true;
            loadTestConfig.Randomize = false;
            loadTestConfig.Timeout = 10;
            loadTestConfig.Server = new List<string>() { "www.yourprimaryserver.com", "www.yoursecondaryserver.com" };
            loadTestConfig.Tag = "Sample Tag";
            loadTestConfig.Sleep = 5;
            loadTestConfig.RunLoop = false;
            loadTestConfig.Duration = 60;
            loadTestConfig.MaxErrors = 10;
            loadTestConfig.DryRun = false;
        }
    }
}
