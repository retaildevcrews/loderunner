// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using LodeRunner.API.Models;
using LodeRunner.API.Test.IntegrationTests.Controllers;
using LodeRunner.API.Test.IntegrationTests.Extensions;
using LodeRunner.Core.Models;
using Xunit;
using Xunit.Abstractions;

namespace LodeRunner.API.Test.IntegrationTests.ExecutingTestRun
{
    /// <summary>
    /// Represents LoadTestConfigs.
    /// </summary>
    public class TestRunExecution : IClassFixture<ApiWebApplicationFactory<Startup>>
    {
        private const string TestRunsUri = "/api/TestRuns";

        private readonly ApiWebApplicationFactory<Startup> factory;

        private readonly JsonSerializerOptions jsonOptions;

        private readonly ITestOutputHelper output;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestRunExecution"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="output">The output.</param>
        public TestRunExecution(ApiWebApplicationFactory<Startup> factory, ITestOutputHelper output)
        {
            this.factory = factory;

            this.output = output;

            this.jsonOptions = new ()
            {
                IgnoreNullValues = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
            };
            this.jsonOptions.Converters.Add(new JsonStringEnumConverter());
        }

        /// <summary>
        /// Determines whether this instance [can create and execute test run].
        /// </summary>
        /// <returns><see cref="Task"/> representing the asynchronous integration test.</returns>
        [Trait("Category", "Integration")]
        [Fact]
        public async Task CanCreateAndExecuteTestRun()
        {
            using var httpClient = ComponentsFactory.CreateLodeRunnerAPIHttpClient(this.factory);

            // Execute dotnet run against LodeRunner project in Client Mode
            string secretsVolume = "secrets".GetSecretVolume();
            using var lodeRunnerAppContext = new ProcessContext(
                new ProcessContextParams()
                {
                    ProjectBasePath = "LodeRunner/LodeRunner.csproj",
                    ProjectArgs = $"--mode Client --secrets-volume {secretsVolume}",
                    ProjectBaseParentDirectoryName = "src",
                }, this.output);

            int apiPortNumber = 8085;

            using var lodeRunnerAPIContext = new ProcessContext(
                new ProcessContextParams()
                {
                    ProjectBasePath = "LodeRunner.API/LodeRunner.API.csproj",
                    ProjectArgs = $"--port {apiPortNumber} --secrets-volume {secretsVolume}",
                    ProjectBaseParentDirectoryName = "src",
                }, this.output);

            string gottenTestRunId = string.Empty;

            try
            {
                this.output.WriteLine($"Starting LodeRunner Application (client mode)");

                this.output.WriteLine($"Starting LodeRunner API.");

                if (lodeRunnerAppContext.Start() && lodeRunnerAPIContext.Start())
                {
                    int apiListeningOnPort = await this.TryParseProcessOutputAndGetAPIListeningPort(lodeRunnerAPIContext.Output);

                    // We should not have any error at time we are going to Verify Id
                    Assert.True(lodeRunnerAppContext.Errors.Count == 0, $"Errors found in LodeRunner Output.{Environment.NewLine}{string.Join(",", lodeRunnerAppContext.Errors)}");

                    Assert.True(apiListeningOnPort == apiPortNumber, "Unable to get Port Number");

                    string clientStatusId = await this.TryParseProcessOutputAndGetClientStatusId(lodeRunnerAppContext.Output);

                    Assert.True(lodeRunnerAPIContext.Errors.Count == 0, $"Errors found in LodeRunner API Output.{Environment.NewLine}{string.Join(",", lodeRunnerAPIContext.Errors)}");

                    Assert.False(string.IsNullOrEmpty(clientStatusId), "Unable to get ClientStatusId");

                    // Verify that clientStatusId exist is Database.
                    await this.VerifyLodeRunnerClientStatusIsReady(httpClient, clientStatusId);

                    // Create Test Run
                    TestRunPayload testRunPayload = new ();

                    string testRunName = $"Sample TestRun - IntegrationTesting-{nameof(this.CanCreateAndExecuteTestRun)}-{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}";

                    int loadClientCount = 1;
                    testRunPayload.SetMockDataToLoadTestLodeRunnerApi(testRunName, clientStatusId, apiListeningOnPort, loadClientCount);

                    HttpResponseMessage postedResponse = await httpClient.PostEntity<TestRun, TestRunPayload>(testRunPayload, TestRunsUri, this.output);
                    Assert.Equal(HttpStatusCode.Created, postedResponse.StatusCode);

                    // Validate Test Run Entity
                    var postedTestRun = await postedResponse.Content.ReadFromJsonAsync<TestRun>(this.jsonOptions);
                    var gottenHttpResponse = await httpClient.GetItemById<TestRun>(TestRunsUri, postedTestRun.Id, this.output);

                    Assert.Equal(HttpStatusCode.OK, gottenHttpResponse.StatusCode);
                    var gottenTestRun = await gottenHttpResponse.Content.ReadFromJsonAsync<TestRun>(this.jsonOptions);

                    Assert.Equal(JsonSerializer.Serialize(postedTestRun), JsonSerializer.Serialize(gottenTestRun));

                    gottenTestRunId = gottenTestRun.Id;

                    // Attempt to get TestRun for N retries or until condition has met.
                    (HttpStatusCode testRunStatusCode, TestRun readyTestRun) = await httpClient.GetEntityByIdRetries<TestRun>(TestRunsUri, postedTestRun.Id, this.jsonOptions, this.output, this.ValidateCompletedTime, 10, 2000);

                    // Validate results
                    Assert.Equal(HttpStatusCode.OK, testRunStatusCode);
                    Assert.True(readyTestRun.CompletedTime != null, "CompletedTime is null.");
                    Assert.True(readyTestRun.LoadClients.Count == loadClientCount, $"LoadClients.Count do not match the expected value [{loadClientCount}]");
                    Assert.True(readyTestRun.ClientResults.Count == readyTestRun.LoadClients.Count, "ClientResults.Count do not match LoadClients.Count");
                    var clientResult = readyTestRun.ClientResults[0];
                    Assert.True(clientResult.TotalRequests == clientResult.FailedRequests + clientResult.SuccessfulRequests, $"TotalRequests {clientResult.TotalRequests} does not match expected value {clientResult.FailedRequests + clientResult.SuccessfulRequests}");

                    this.output.WriteLine($"TestRun passed validation.");

                    // Verify that ClientStatus is Ready again.
                    await this.VerifyLodeRunnerClientStatusIsReady(httpClient, clientStatusId);

                    // End LodeRunner Context.
                    lodeRunnerAppContext.End();
                    this.output.WriteLine($"Stopping LodeRunner Application (client mode) [ClientStatusId: {clientStatusId}]");

                    lodeRunnerAPIContext.End();
                    this.output.WriteLine($"Stopping LodeRunner API.");
                }
                else
                {
                    Assert.True(false, "Unable to start LodeRunner App Context.");
                }
            }
            finally
            {
                // gottenTestRunId gets set only after successfully have gotten and validated the Test Run entity.
                if (!string.IsNullOrEmpty(gottenTestRunId))
                {
                    var response = await httpClient.DeleteItemById<TestRun>(TestRunsUri, gottenTestRunId, this.output);

                    // The Delete action should success because we are validating "testRun.CompletedTime" at this.ValidateCompletedTime
                    Assert.NotEqual(HttpStatusCode.Conflict, response.StatusCode);
                }
            }
        }

        /// <summary>
        /// Gets the substring by string.
        /// </summary>
        /// <param name="openString">The open string.</param>
        /// <param name="closingString">The closing string.</param>
        /// <param name="baseString">The base string.</param>
        /// <returns>substring.</returns>
        private static string GetSubstringByString(string openString, string closingString, string baseString)
        {
            return baseString.Substring(baseString.IndexOf(openString) + openString.Length, baseString.IndexOf(closingString) - baseString.IndexOf(openString) - openString.Length);
        }

        /// <summary>
        /// Verifies the lode runner client status is ready.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="clientStatusId">The client status identifier.</param>
        private async Task VerifyLodeRunnerClientStatusIsReady(HttpClient httpClient, string clientStatusId)
        {
            (HttpStatusCode currentClientStatusCode, Client currentClient) = await httpClient.GetClientByIdRetriesAsync(Clients.ClientsUri, clientStatusId, ClientStatusType.Ready, this.jsonOptions, this.output, 10, 1000);

            Assert.True(currentClientStatusCode == HttpStatusCode.OK, $"Invalid response status code: {currentClientStatusCode}");
            Assert.True(currentClient != null, "Unable to get Client entity.");
            Assert.Equal(clientStatusId, currentClient.ClientStatusId);
            Assert.Equal(ClientStatusType.Ready, currentClient.Status);
        }

        /// <summary>
        /// Validates the completed time.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <returns>The Task.</returns>
        private async Task<(bool result, string fieldName, string conditionalValue)> ValidateCompletedTime(TestRun testRun)
        {
            return await Task.Run(() =>
            {
                return (testRun.CompletedTime != null, "CompletedTime", testRun.CompletedTime == null ? "null" : testRun.CompletedTime.ToString());
            });
        }

        /// <summary>
        /// Parses the process output and to get Port number that the API is listening on.
        /// </summary>
        /// <param name="outputList">The Process outputList.</param>
        /// <param name="timeBetweenTriesMs">The time between tries ms.</param>
        /// <param name="maxRetries">The maximum retries.</param>
        /// <returns>port number. 0 if not found.</returns>
        private async Task<int> TryParseProcessOutputAndGetAPIListeningPort(List<string> outputList, int timeBetweenTriesMs = 1000, int maxRetries = 20)
        {
            int portNumber = 0;
            for (int i = 1; i <= maxRetries; i++)
            {
                await Task.Delay(timeBetweenTriesMs).ConfigureAwait(false); // Log Interval is 5 secs.

                string targetOutputLine = outputList.FirstOrDefault(s => s.Contains("Now listening on:"));
                if (!string.IsNullOrEmpty(targetOutputLine))
                {
                    // the line should look like "Now listening on: http://[::]:8080",  since we are splitting on ":" last string in the list will be the port either 8080 (Production) or 8081 (Development)
                    string portNumberString = targetOutputLine.Split(":").ToList().LastOrDefault();

                    if (int.TryParse(portNumberString, out portNumber))
                    {
                        this.output.WriteLine($"UTC Time:{DateTime.UtcNow}\tParsing LodeRunner API Process Output.\tPort Number Found.\tPort: '{portNumber}'\tAttempts: {i} [{timeBetweenTriesMs}ms between requests]");
                    }

                    return portNumber;
                }
                else
                {
                    this.output.WriteLine($"UTC Time:{DateTime.UtcNow}\tParsing LodeRunner API Process Output.\tPort Number Not Found.\tAttempts: {i} [{timeBetweenTriesMs}ms between requests]");
                }
            }

            return portNumber;
        }

        /// <summary>
        /// Parses the process output and to get client status id.
        /// </summary>
        /// <param name="outputList">The Process outputList.</param>
        /// <param name="timeBetweenTriesMs">The time between tries ms.</param>
        /// <param name="maxRetries">The maximum retries.</param>
        /// <returns>the ClientStatusId.</returns>
        private async Task<string> TryParseProcessOutputAndGetClientStatusId(List<string> outputList, int timeBetweenTriesMs = 500, int maxRetries = 10)
        {
            string clientStatusId = null;
            for (int i = 1; i <= maxRetries; i++)
            {
                await Task.Delay(timeBetweenTriesMs).ConfigureAwait(false); // Log Interval is 5 secs.

                string targetOutputLine = outputList.FirstOrDefault(s => s.Contains(LodeRunner.Core.SystemConstants.InitializingClient));
                if (!string.IsNullOrEmpty(targetOutputLine))
                {
                    clientStatusId = GetSubstringByString("(", ")", targetOutputLine);
                    this.output.WriteLine($"UTC Time:{DateTime.UtcNow}\tParsing LodeRunner Process Output.\tClientStatusId Found.\tId: '{clientStatusId}'\tAttempts: {i} [{timeBetweenTriesMs}ms between requests]");
                    return clientStatusId;
                }
                else
                {
                    this.output.WriteLine($"UTC Time:{DateTime.UtcNow}\tParsing LodeRunner Process Output.\tClientStatusId Not Found.\tAttempts: {i} [{timeBetweenTriesMs}ms between requests]");
                }
            }

            return clientStatusId;
        }
    }
}
