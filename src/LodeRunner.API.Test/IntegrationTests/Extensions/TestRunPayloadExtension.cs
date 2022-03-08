// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using LodeRunner.Core.Models;

namespace LodeRunner.API.Test.IntegrationTests
{
    /// <summary>
    /// TestRunPayload Extensions.
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
            testRunPayload.SetNameAndTime(name);

            testRunPayload.LoadTestConfig = CreateLoadTestConfig();

            testRunPayload.LoadClients = CreateLoadClients(2);
        }

        /// <summary>
        /// Sets the mock data to load test LodeRunner API.
        /// </summary>
        /// <param name="testRunPayload">The test run payload.</param>
        /// <param name="name">The name.</param>
        /// <param name="loadClientId">The loadClient Id from LodeRunner Client.</param>
        /// <param name="apiServerPort">The list of API ports for servers to target during load test.</param>
        public static void SetMockDataToLoadTestLodeRunnerApi(this TestRunPayload testRunPayload, string name, string loadClientId, List<int> apiServerPort)
        {
            testRunPayload.SetNameAndTime(name);

            var loadTestConfig = new LoadTestConfig()
            {
                Name = $"Sample LoadTestConfig - IntegrationTesting-{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}",
                Files = new List<string>() { "LodeRunner.Api-benchmark.json" },
                Server = new List<string>(),
            };

            foreach (var hostPort in apiServerPort)
            {
                loadTestConfig.Server.Add(string.Format(SystemConstants.BaseUriLocalHostPort, hostPort));
            }

            testRunPayload.LoadTestConfig = loadTestConfig;

            testRunPayload.LoadClients = CreateLoadClients(1, loadClientId);
        }

        /// <summary>
        /// Sets the name and time.
        /// </summary>
        /// <param name="testRunPayload">The test run payload.</param>
        /// <param name="name">The name.</param>
        private static void SetNameAndTime(this TestRunPayload testRunPayload, string name)
        {
            testRunPayload.Name = name;
            testRunPayload.CreatedTime = DateTime.UtcNow;
            testRunPayload.StartTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates a load test configuration.
        /// </summary>
        /// <returns>A new LoadTestConfig with mock data.</returns>
        private static LoadTestConfig CreateLoadTestConfig()
        {
            var result = new LoadTestConfig();
            result.SetMockData($"Sample LoadTestConfig - IntegrationTesting-{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}");
            return result;
        }

        /// <summary>
        /// Creates the load clients.
        /// </summary>
        /// <param name="count">The count.</param>
        /// <param name="loadClientId">The loadClient Id from LodeRunner Client.</param>
        /// <returns>List of N clients with mock data.</returns>
        private static List<LoadClient> CreateLoadClients(int count, string loadClientId = null)
        {
            var result = new List<LoadClient>();

            for (int i = 1; i <= count; i++)
            {
                var mockedLoadClient = new LoadClient();

                if (!string.IsNullOrEmpty(loadClientId))
                {
                    mockedLoadClient.Id = loadClientId;
                }

                mockedLoadClient.SetMockData($"Sample LoadClient [{i}] - IntegrationTesting-{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}");
                result.Add(mockedLoadClient);
            }

            return result;
        }
    }
}
