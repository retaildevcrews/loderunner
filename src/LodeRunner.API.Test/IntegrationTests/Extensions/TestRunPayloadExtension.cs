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

            // Initialize a loadClient list with 2 random Guids just to mock data, we do not really care about the loadClientId in this case.
            List<string> loadClientIds = new() { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };

            testRunPayload.LoadClients = CreateLoadClients(loadClientIds);
        }

        /// <summary>
        /// Sets the mock data to load test LodeRunner API.
        /// </summary>
        /// <param name="testRunPayload">The test run payload.</param>
        /// <param name="name">The name.</param>
        /// <param name="loadClientIds">The loadClient Id list from real up and running LodeRunner Client Instances.</param>
        /// <param name="apiServerPort">The list of API ports for servers to target during load test.</param>
        /// <param name="sleepMs">The sleep time between requests in ms.</param>
        /// <param name="runLoop">Detemines if should run in loop.</param>
        public static void SetMockDataToLoadTestLodeRunnerApi(this TestRunPayload testRunPayload, string name, List<string> loadClientIds, List<int> apiServerPort, int sleepMs = 0, bool runLoop = false)
        {
            testRunPayload.SetNameAndTime(name);

            var loadTestConfig = new LoadTestConfig()
            {
                Name = $"Sample LoadTestConfig - IntegrationTesting-{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}",
                Files = new List<string>() { "LodeRunner.Api-benchmark.json" },
                Server = new List<string>(),
                Sleep = sleepMs,
                RunLoop = runLoop,
                Duration = runLoop ? 60 : 0,
            };

            foreach (var hostPort in apiServerPort)
            {
                loadTestConfig.Server.Add(string.Format(SystemConstants.BaseUriLocalHostPort, hostPort));
            }

            testRunPayload.LoadTestConfig = loadTestConfig;

            testRunPayload.LoadClients = CreateLoadClients(loadClientIds);
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
        /// <param name="loadClientIdList">The loadClient Id from LodeRunner Client.</param>
        /// <returns>List of N clients with mock data.</returns>
        private static List<LoadClient> CreateLoadClients(List<string> loadClientIdList)
        {
            var result = new List<LoadClient>();
            int loadClientCount = 0;
            foreach (var loadClientId in loadClientIdList)
            {
                var mockedLoadClient = new LoadClient
                {
                    Id = loadClientId,
                };

                loadClientCount++;

                mockedLoadClient.SetMockData($"Sample LoadClient [{loadClientCount}] - IntegrationTesting-{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}");
                result.Add(mockedLoadClient);
            }

            return result;
        }
    }
}
