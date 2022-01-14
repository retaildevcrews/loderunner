﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LodeRunner.API.Models;
using LodeRunner.API.Test.IntegrationTests.Payloads;
using LodeRunner.Core.Models;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace LodeRunner.API.Test.IntegrationTests
{
    /// <summary>
    /// Http Client Extensions.
    /// </summary>
    public static class HttpClientExtension
    {
        private const int WaitingTimeIncrementMs = 100;

        /// <summary>
        /// Validate GetById Request.
        /// </summary>
        /// <param name="httpClient">the httpClient.</param>
        /// <param name="clientsByIdUri">the clientsById Uri.</param>
        /// <param name="clientStatusId">the clientStatusId.</param>
        /// <returns>HttpResponseMessage.</returns>
        public static async Task<HttpResponseMessage> ValidateAndGetByIdRequest(this HttpClient httpClient, string clientsByIdUri, string clientStatusId)
        {
            HttpResponseMessage httpResponse = await httpClient.GetAsync($"{clientsByIdUri}{clientStatusId}");

            Assert.False(httpResponse.StatusCode == HttpStatusCode.NoContent, "Response Code 204 - No Content.");

            return httpResponse;
        }

        /// <summary>
        /// Waits and validate GetClients Response to match ClientStatus for the given ClientStatusId.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="clientsByIdUri">The clients by identifier URI.</param>
        /// <param name="clientStatusId">The client status identifier.</param>
        /// <param name="clientStatusType">Type of the client status.</param>
        /// <param name="jsonOptions">The json options.</param>
        /// <param name="output">The output.</param>
        /// <param name="timeoutLimitMs">The timeout limit ms.</param>
        /// <returns>The task.</returns>
        public static async Task<bool> WaitAndValidateGetByIdToMatchStatus(this HttpClient httpClient, string clientsByIdUri, string clientStatusId, ClientStatusType clientStatusType, JsonSerializerOptions jsonOptions, ITestOutputHelper output, int timeoutLimitMs = 1000)
        {
            int timeout = WaitingTimeIncrementMs;

            bool foundAndValid = false;

            HttpStatusCode lastHttpCode = HttpStatusCode.Unused;

            while (timeout <= timeoutLimitMs)
            {
                var httpResponse = await ValidateAndGetByIdRequest(httpClient, clientsByIdUri, clientStatusId);

                lastHttpCode = httpResponse.StatusCode;

                if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    output.WriteLine($"Response StatusCode: 'NotFound' \t Delay: {timeout} ms \tClientStatusId: '{clientStatusId}' ");
                    await Task.Delay(WaitingTimeIncrementMs);
                    timeout += WaitingTimeIncrementMs;
                }
                else if (httpResponse.IsSuccessStatusCode)
                {
                    var client = await httpResponse.Content.ReadFromJsonAsync<Client>(jsonOptions);

                    foundAndValid = client != null && client.Status == clientStatusType;

                    if (foundAndValid)
                    {
                        output.WriteLine($"Local Time:{DateTime.Now}\tClientStatusType: '{clientStatusType}' ClientStatusId: '{clientStatusId}' \tfound within: {timeout} ms");
                        break;
                    }
                    else
                    {
                        output.WriteLine($"Response StatusCode: 'OK'\t Delay: {timeout} ms \tClientStatusId: '{clientStatusId}' \tClientStatusType: '{client?.Status}'");
                        await Task.Delay(WaitingTimeIncrementMs);
                        timeout += WaitingTimeIncrementMs;
                    }
                }
                else
                {
                    // break while loop response was not successful.
                    break;
                }
            }

            Assert.True(foundAndValid, $"Local Time:{DateTime.Now}\tUnable to process request for ClientStatusId: {clientStatusId}\tClientStatusType: '{clientStatusType}' - HttpStatusCode:{lastHttpCode} - Timeout:{timeout} ms");

            return foundAndValid;
        }

        /// <summary>
        /// Waits and validates GetClients Response to match ClientStatusId.
        /// </summary>
        /// <param name="httpClient">the httpClient.</param>
        /// <param name="clientsUri">clientsById Uri.</param>
        /// <param name="clientStatusId">The client status identifier.</param>
        /// <param name="jsonOptions">The json options.</param>
        /// <param name="output">The output.</param>
        /// <param name="timeoutLimitMs">The timeout limit ms.</param>
        /// <returns>the task.</returns>
        public static async Task<bool> WaitAndValidateGetClientsToMatchId(this HttpClient httpClient, string clientsUri, string clientStatusId, JsonSerializerOptions jsonOptions, ITestOutputHelper output, int timeoutLimitMs = 1000)
        {
            int timeout = WaitingTimeIncrementMs;

            bool found = false;

            HttpStatusCode lastHttpCode = HttpStatusCode.Unused;

            while (timeout <= timeoutLimitMs)
            {
                var httpResponse = await httpClient.GetAsync(clientsUri);

                lastHttpCode = httpResponse.StatusCode;

                if (httpResponse.StatusCode == HttpStatusCode.NoContent)
                {
                    output.WriteLine($"Delay: {WaitingTimeIncrementMs} ms");
                    await Task.Delay(WaitingTimeIncrementMs);
                    timeout += WaitingTimeIncrementMs;
                }
                else if (httpResponse.IsSuccessStatusCode)
                {
                    var clients = await httpResponse.Content.ReadFromJsonAsync<IEnumerable<Client>>(jsonOptions);

                    found = clients.Any(x => x.ClientStatusId == clientStatusId);

                    if (found)
                    {
                        output.WriteLine($"Local Time:{DateTime.Now}\tClientStatusId: '{clientStatusId}' found within {timeout} ms");

                        break;
                    }
                    else
                    {
                        output.WriteLine($"Delay: {timeout} ms");
                        await Task.Delay(WaitingTimeIncrementMs);
                        timeout += WaitingTimeIncrementMs;
                    }
                }
                else
                {
                    // break while loop response was not successful.
                    break;
                }
            }

            Assert.True(found, $"Local Time:{DateTime.Now}\tUnable to process GetClients request, it could not verify ClientStatusId: {clientStatusId}\t- HttpStatusCode:{lastHttpCode} - Timeout:{timeout} ms");

            return found;
        }

        /// <summary>
        /// GetTestRuns.
        /// </summary>
        /// <param name="httpClient">the httpClient.</param>
        /// <param name="getTestRunsUri">clientsById Uri.</param>
        /// <param name="jsonOptions">The json options.</param>
        /// <param name="output">The output.</param>
        /// <returns>the task.</returns>
        public static async Task<bool> GetTestRuns(this HttpClient httpClient, string getTestRunsUri, JsonSerializerOptions jsonOptions, ITestOutputHelper output)
        {
            bool found = false;

            var httpResponse = await httpClient.GetAsync(getTestRunsUri);

            if (httpResponse.IsSuccessStatusCode && httpResponse.StatusCode == HttpStatusCode.OK)
            {
                var testRuns = await httpResponse.Content.ReadFromJsonAsync<IEnumerable<TestRun>>(jsonOptions);

                found = testRuns.Any();

                if (found)
                {
                    output.WriteLine($"Local Time:{DateTime.Now}\t[{testRuns.Count()}] Test Runs found.");
                }
            }

            // TestRunsController will return 404 if not TestRuns items found after a request was successfully processed,  so 404 it is a valid response code,
            else if (httpResponse.StatusCode == HttpStatusCode.NoContent)
            {
                found = true;
            }

            Assert.True(found, $"Local Time:{DateTime.Now}\tUnable to Get any TestRuns");

            return found;
        }

        /// <summary>
        /// Gets the test runs.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="getTestRunByIdUri">The get test runs URI.</param>
        /// <param name="jsonOptions">The json options.</param>
        /// <returns>the task.</returns>
        public static async Task<TestRun> GetTestRunById(this HttpClient httpClient, string getTestRunByIdUri, JsonSerializerOptions jsonOptions)
        {
            var httpResponse = await httpClient.GetAsync(getTestRunByIdUri);

            if (httpResponse.IsSuccessStatusCode && httpResponse.StatusCode == HttpStatusCode.OK)
            {
                var testRun = await httpResponse.Content.ReadFromJsonAsync<TestRun>(jsonOptions);

                Assert.True(testRun != null, "Unable to get test run");

                return testRun;
            }

            return null;
        }

        /// <summary>
        /// GetTestRuns.
        /// </summary>
        /// <param name="httpClient">the httpClient.</param>
        /// <param name="postTestRunsUri">clientsById Uri.</param>
        /// <param name="jsonOptions">The json options.</param>
        /// <param name="output">The output.</param>
        /// <returns>the task.</returns>
        public static async Task<TestRun> PostTestRun(this HttpClient httpClient, string postTestRunsUri, JsonSerializerOptions jsonOptions, ITestOutputHelper output)
        {
            TestRunTestPayload testRunTestPayload = new ();

            testRunTestPayload.Name = $"Sample TestRun - IntegrationTesting-{nameof(PutTestRun)}-{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}";

            string jsonTestRun = JsonConvert.SerializeObject(testRunTestPayload);
            StringContent stringContent = new (jsonTestRun, Encoding.UTF8, "application/json");

            var httpResponse = await httpClient.PostAsync(postTestRunsUri, stringContent);

            if (httpResponse.IsSuccessStatusCode)
            {
                var testRun = await httpResponse.Content.ReadFromJsonAsync<TestRun>(jsonOptions);

                return testRun;
            }

            return null;
        }

        /// <summary>
        /// GetTestRuns.
        /// </summary>
        /// <param name="httpClient">the httpClient.</param>
        /// <param name="testRunPayload">the testRunPayload entity.</param>
        /// <param name="testRunId">clientsById Uri.</param>
        /// <param name="testRunsUri">The test run id.</param>
        /// <param name="jsonOptions">The json options.</param>
        /// <param name="output">The output.</param>
        /// <returns>the task.</returns>
        public static async Task<bool> PutTestRun(this HttpClient httpClient, TestRunPayload testRunPayload, string testRunId, string testRunsUri, JsonSerializerOptions jsonOptions, ITestOutputHelper output)
        {
            bool valid = false;
            string newName = $"Updated TestRun - IntegrationTesting-{nameof(PutTestRun)}-{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}";

            testRunPayload.Name = newName;

            int actualLoadClientsCount = testRunPayload.LoadClients.Count;

            Assert.False(testRunPayload.LoadClients.Count == 0, "TestRunPayload is expecting to have at least 1 item, since it is a TestPayload sample, look at [TestRunTestPayload.cs] file.");

            testRunPayload.LoadClients.Add(testRunPayload.LoadClients[0].AutomapAndGetaNewLoadClient());

            string jsonTestRun = JsonConvert.SerializeObject(testRunPayload);

            StringContent stringContent = new (jsonTestRun, Encoding.UTF8, "application/json");

            var httpResponse = await httpClient.PutAsync($"{testRunsUri}/{testRunId}", stringContent);

            if (httpResponse.IsSuccessStatusCode)
            {
                Assert.True(httpResponse.StatusCode == HttpStatusCode.NoContent, "Invalid status code.");

                var updatedTestRun = await httpClient.GetTestRunById($"{testRunsUri}/{testRunId}", jsonOptions);

                Assert.NotNull(updatedTestRun);

                Assert.Equal(newName, updatedTestRun.Name);

                Assert.Equal(actualLoadClientsCount + 1, updatedTestRun.LoadClients.Count);

                valid = true;
            }

            Assert.True(valid, $"Local Time:{DateTime.Now}\tUnable to Get TestRun");

            return valid;
        }
    }
}
