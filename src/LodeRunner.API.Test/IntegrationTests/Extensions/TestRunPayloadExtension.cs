// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using LodeRunner.Core.Models;

namespace LodeRunner.API.Test.IntegrationTests
{
    /// <summary>
    /// TestRunPayload Extentions.
    /// </summary>
    public static class TestRunPayloadExtension
    {
        /// <summary>
        /// Sets mock TestRun payload data.
        /// </summary>
        /// <param name="testRunPayload">TestRunPayload.</param>
        /// <param name="name">TestRun name.</param>
        public static void SetMockData(this TestRunPayload testRunPayload, string name)
        {
            testRunPayload.Name = name;
            testRunPayload.CreatedTime = DateTime.UtcNow;
            testRunPayload.StartTime = DateTime.UtcNow;

            testRunPayload.LoadTestConfig = new LoadTestConfig();
            testRunPayload.LoadTestConfig.SetMockData($"Sample LoadTestConfig - IntegrationTesting-{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}");

            var mockedLoadClientA = new LoadClient();
            mockedLoadClientA.SetMockData($"Sample LoadClient A - IntegrationTesting-{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}");

            var mockedLoadClientB = new LoadClient();
            mockedLoadClientB.SetMockData($"Sample LoadClient B - IntegrationTesting-{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}");

            testRunPayload.LoadClients = new List<LoadClient>
            {
                mockedLoadClientA,
                mockedLoadClientB,
            };
        }
    }
}
