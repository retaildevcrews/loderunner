// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using LodeRunner.Core.Models;

namespace LodeRunner.API.Test.IntegrationTests
{
    /// <summary>
    /// LoadClient Extension.
    /// </summary>
    public static class LoadClientExtension
    {
        /// <summary>
        /// Sets mock LoadClient data.
        /// </summary>
        /// <param name="loadClient">LoadClient.</param>
        /// <param name="name">LoadClient name.</param>
        public static void SetMockData(this LoadClient loadClient, string name)
        {
            loadClient.Name = name;
            loadClient.Version = "1.0.1";
            loadClient.Region = "Central";
            loadClient.Zone = "central-az-1";
            loadClient.Prometheus = true;
            loadClient.StartupArgs = "--mode Client --region Central --zone central-az-1 --prometheus true";
            loadClient.StartTime = DateTime.UtcNow;
        }
    }
}
