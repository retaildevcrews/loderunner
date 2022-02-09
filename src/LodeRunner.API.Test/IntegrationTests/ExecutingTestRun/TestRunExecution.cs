// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
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
            string cmdLine = "dotnet";

            string args = $"run --project ../../../../LodeRunner/LodeRunner.csproj --mode Client --secrets-volume secrets";
            using var lodeRunnerAppContext = new ProcessContext(cmdLine, args, this.output);

            string gottenTestRunId = string.Empty;

            try
            {
                if (lodeRunnerAppContext.Start())
                {
                    string clientStatusId = await this.TryParseProcessOutputAndGetClientStatusId(lodeRunnerAppContext.Output);

                    Assert.False(string.IsNullOrEmpty(clientStatusId), "Unable to get ClientStatusId");

                    // We should not have any error at time we are going to Verify Id
                    Assert.True(lodeRunnerAppContext.Errors.Count == 0, $"Errors found in LodeRunner Output.{Environment.NewLine}{string.Join(",", lodeRunnerAppContext.Errors)}");

                    // Verify that clientStatusId exist is Database.
                    (HttpStatusCode clientStatusCode, Client readyClient) = await httpClient.GetClientByIdRetriesAsync(Clients.ClientsUri, clientStatusId, ClientStatusType.Ready, this.jsonOptions, this.output, 10, 1000);

                    Assert.True(clientStatusCode == HttpStatusCode.OK, $"Invalid response status code: {clientStatusCode}");
                    Assert.True(readyClient != null, "Unable to get Client entity.");
                    Assert.Equal(clientStatusId, readyClient.ClientStatusId);

                    // Create Test Run
                    TestRunPayload testRunPayload = new ();

                    testRunPayload.SetMockDataToLoadTestLodeRunnerApi($"Sample TestRun - IntegrationTesting-{nameof(this.CanCreateAndExecuteTestRun)}-{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}");

                    HttpResponseMessage postedResponse = await httpClient.PostEntity<TestRun, TestRunPayload>(testRunPayload, TestRunsUri, this.output);
                    Assert.Equal(HttpStatusCode.Created, postedResponse.StatusCode);

                    // Validate Test Run Entity
                    var postedTestRun = await postedResponse.Content.ReadFromJsonAsync<TestRun>(this.jsonOptions);
                    var gottenHttpResponse = await httpClient.GetItemById<TestRun>(TestRunsUri, postedTestRun.Id, this.output);

                    Assert.Equal(HttpStatusCode.OK, gottenHttpResponse.StatusCode);
                    var gottenTestRun = await gottenHttpResponse.Content.ReadFromJsonAsync<TestRun>(this.jsonOptions);

                    Assert.Equal(JsonSerializer.Serialize(postedTestRun), JsonSerializer.Serialize(gottenTestRun));

                    gottenTestRunId = gottenTestRun.Id;

                    // Get TestRun with retries or until condition has met.
                    (HttpStatusCode testRunStatusCode, TestRun readyTestRun) = await httpClient.GetEntityByIdRetries<TestRun>(TestRunsUri, postedTestRun.Id, this.jsonOptions, this.output, this.ValidateCompletedTime, 10, 2000);

                    // Validate results
                    Assert.Equal(HttpStatusCode.OK, testRunStatusCode);
                    Assert.True(readyTestRun.CompletedTime != null, "CompletedTime is null.");
                    Assert.True(readyTestRun.ClientResults.Count == readyTestRun.LoadClients.Count, "ClientResults.Count do not match LoadClients.Count");

                    // End LodeRunner Context.
                    lodeRunnerAppContext.End();
                    this.output.WriteLine($"Stopping LodeRunner Application (client mode) [ClientStatusId: {clientStatusId}]");
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
        /// Validates the completed time.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <returns>The Task.</returns>
        private async Task<(bool result, string fieldName, string conditionalValue)> ValidateCompletedTime(TestRun testRun)
        {
            return await Task.Run(() =>
            {
                return (testRun.CompletedTime != null, "CompletedTime", testRun.CompletedTime.ToString());
            });
        }

        /// <summary>
        /// Parses the process output and to get client status id.
        /// </summary>
        /// <param name="outputList">The Process outputList.</param>
        /// <param name="maxRetries">The maximum retries.</param>
        /// <param name="timeBetweenTriesMs">The time between tries ms.</param>
        /// <returns>the ClientStatusId.</returns>
        private async Task<string> TryParseProcessOutputAndGetClientStatusId(List<string> outputList, int maxRetries = 10, int timeBetweenTriesMs = 2000)
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
