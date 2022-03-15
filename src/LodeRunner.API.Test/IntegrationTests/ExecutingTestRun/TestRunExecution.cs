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
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.API.Models;
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
        /// Determines whether this instance [can create and execute test run with a given number of API hosts].
        /// </summary>
        /// <param name="apiHostCount">The number API hosts to utilized.</param>
        /// <returns><see cref="Task"/> representing the asynchronous integration test.</returns>
        [Trait("Category", "Integration")]
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task CanCreateAndExecuteTestRun(int apiHostCount)
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

            using var apiProcessContextCollection = new ApiProcessContextCollection(apiHostCount, secretsVolume, this.output);

            string gottenTestRunId = string.Empty;

            try
            {
                this.output.WriteLine($"Starting LodeRunner Application (client mode)");

                Assert.True(lodeRunnerAppContext.Start(), "Unable to start LodeRunner App Context.");

                string lodeRunnerServiceLogName = "LodeRunner.Services.LodeRunnerService";

                // Get LodeRunner Client Status Id.
                string clientStatusId = await this.TryParseProcessOutputAndGetValueFromFieldName(lodeRunnerAppContext.Output, lodeRunnerServiceLogName, LodeRunner.Core.SystemConstants.InitializingClient, LodeRunner.Core.SystemConstants.ClientStatusIdFieldName);
                Assert.False(string.IsNullOrEmpty(clientStatusId), "Unable to get ClientStatusId when Initializing Client.");

                // Get LodeRunner LoadClient Id.
                string loadClientId = await this.TryParseProcessOutputAndGetValueFromFieldName(lodeRunnerAppContext.Output, lodeRunnerServiceLogName, LodeRunner.Core.SystemConstants.InitializingClient, LodeRunner.Core.SystemConstants.LoadClientIdFieldName, 10, apiHostCount * 2000);
                Assert.False(string.IsNullOrEmpty(loadClientId), "Unable to get loadClientId");

                // We should not have any error at time we are going to Verify Id
                Assert.True(lodeRunnerAppContext.Errors.Count == 0, $"Errors found in LodeRunner Output.{Environment.NewLine}{string.Join(",", lodeRunnerAppContext.Errors)}");

                Assert.True(apiProcessContextCollection.Start(this.factory.GetNextAvailablePort), $"Api ProcessContext Collection.");

                List<int> portList = new ();

                foreach (var apiProcessContext in apiProcessContextCollection)
                {
                    this.output.WriteLine($"Starting LodeRunner API for Host {apiProcessContext.hostId}.");

                    Assert.True(apiProcessContext.apiProcessContext.Started, $"Unable to start LodeRunner API Context for Host {apiProcessContext.hostId}.");

                    Assert.True(apiProcessContext.apiProcessContext.Errors.Count == 0, $"Errors found in LodeRunner API - Host {apiProcessContext.hostId} Output.{Environment.NewLine}{string.Join(",", apiProcessContext.apiProcessContext.Errors)}");

                    int apiListeningOnPort = await this.TryParseProcessOutputAndGetAPIListeningPort(apiProcessContext.apiProcessContext.Output);

                    Assert.True(apiListeningOnPort == apiProcessContext.portNumber, "Unable to get Port Number");

                    portList.Add(apiListeningOnPort);
                }

                // Verify that clientStatusId exist is Database.
                await this.VerifyLodeRunnerClientStatusIsReady(httpClient, clientStatusId);

                // Create Test Run
                TestRunPayload testRunPayload = new ();

                string testRunName = $"Sample TestRun - IntegrationTesting-{nameof(this.CanCreateAndExecuteTestRun)}-{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}";

                testRunPayload.SetMockDataToLoadTestLodeRunnerApi(testRunName, loadClientId, portList);

                HttpResponseMessage postedResponse = await httpClient.PostEntity<TestRun, TestRunPayload>(testRunPayload, SystemConstants.CategoryTestRunsPath, this.output);

                AssertExtension.EqualResponseStatusCode(HttpStatusCode.Created, postedResponse);

               // Validate Test Run Entity
                var postedTestRun = await postedResponse.Content.ReadFromJsonAsync<TestRun>(this.jsonOptions);
                var gottenHttpResponse = await httpClient.GetItemById<TestRun>(SystemConstants.CategoryTestRunsPath, postedTestRun.Id, this.output);

                AssertExtension.EqualResponseStatusCode(HttpStatusCode.OK, gottenHttpResponse);

                var gottenTestRun = await gottenHttpResponse.Content.ReadFromJsonAsync<TestRun>(this.jsonOptions);

                Assert.Equal(JsonSerializer.Serialize(postedTestRun), JsonSerializer.Serialize(gottenTestRun));

                gottenTestRunId = gottenTestRun.Id;

                // Get LodeRunner TestRun Id when Received
                string testRunId = await this.TryParseProcessOutputAndGetValueFromFieldName(lodeRunnerAppContext.Output, lodeRunnerServiceLogName, LodeRunner.Core.SystemConstants.ReceivedNewTestRun, LodeRunner.Core.SystemConstants.TestRunIdFieldName, 10, apiHostCount * 3000);
                Assert.False(string.IsNullOrEmpty(testRunId), "Unable to get TestRunId when Received TestRun");

                // Attempt to get TestRun for N retries or until condition has met.
                (HttpResponseMessage testRunResponse, TestRun readyTestRun) = await httpClient.GetEntityByIdRetries<TestRun>(SystemConstants.CategoryTestRunsPath, postedTestRun.Id, this.jsonOptions, this.output, this.ValidateCompletedTime, 10, apiHostCount * 3000);

                // Get LodeRunner TestRun Id when Executing
                testRunId = await this.TryParseProcessOutputAndGetValueFromFieldName(lodeRunnerAppContext.Output, lodeRunnerServiceLogName, LodeRunner.Core.SystemConstants.ExecutingTestRun, LodeRunner.Core.SystemConstants.TestRunIdFieldName, 10, apiHostCount * 3000);
                Assert.False(string.IsNullOrEmpty(testRunId), "Unable to get TestRunId when Executing TestRun.");

                // Validate that all 3 ids were logged in LodeRunner-Command output.
                string lodeRunnerCmdOutputMarker = string.Format(SystemConstants.BaseUriLocalHostPort, portList[0]);
                clientStatusId = await this.TryParseProcessOutputAndGetValueFromFieldName(lodeRunnerAppContext.Output, LodeRunner.Core.SystemConstants.LoadTestRequestLogName, lodeRunnerCmdOutputMarker, LodeRunner.Core.SystemConstants.ClientStatusIdFieldName, 10, 500);
                Assert.False(string.IsNullOrEmpty(clientStatusId), "Unable to get ClientStatusId from LodeRunner-Command output");

                loadClientId = await this.TryParseProcessOutputAndGetValueFromFieldName(lodeRunnerAppContext.Output, LodeRunner.Core.SystemConstants.LoadTestRequestLogName, lodeRunnerCmdOutputMarker, LodeRunner.Core.SystemConstants.LoadClientIdFieldName, 10, 500);
                Assert.False(string.IsNullOrEmpty(loadClientId), "Unable to get LoadClientId from LodeRunner-Command output");

                testRunId = await this.TryParseProcessOutputAndGetValueFromFieldName(lodeRunnerAppContext.Output, LodeRunner.Core.SystemConstants.LoadTestRequestLogName, lodeRunnerCmdOutputMarker, LodeRunner.Core.SystemConstants.TestRunIdFieldName, 10, 500);
                Assert.False(string.IsNullOrEmpty(testRunId), "Unable to get TestRunId from LodeRunner-Command output");

                // Validate results
                int expectedLoadClientCount = 1;
                AssertExtension.EqualResponseStatusCode(HttpStatusCode.OK, testRunResponse);

                Assert.True(readyTestRun.CompletedTime != null, "CompletedTime is null.");
                Assert.True(readyTestRun.LoadClients.Count == expectedLoadClientCount, $"LoadClients.Count do not match the expected value [{expectedLoadClientCount}]");
                Assert.True(readyTestRun.ClientResults.Count == readyTestRun.LoadClients.Count, "ClientResults.Count do not match LoadClients.Count");
                var clientResult = readyTestRun.ClientResults[0];
                Assert.True(clientResult.TotalRequests == clientResult.FailedRequests + clientResult.SuccessfulRequests, $"TotalRequests {clientResult.TotalRequests} does not match expected value {clientResult.FailedRequests + clientResult.SuccessfulRequests}");

                this.output.WriteLine($"TestRun passed validation.");

                // Verify that ClientStatus is Ready again.
                await this.VerifyLodeRunnerClientStatusIsReady(httpClient, clientStatusId);

                // End LodeRunner Context.
                lodeRunnerAppContext.End();
                this.output.WriteLine($"Stopping LodeRunner Application (client mode) [ClientStatusId: {clientStatusId}]");

                foreach (var apiProcessContext in apiProcessContextCollection)
                {
                    Assert.True(apiProcessContext.apiProcessContext.Errors.Count == 0, $"Errors found in LodeRunner API - Host {apiProcessContext.hostId} Output.{Environment.NewLine}{string.Join(",", apiProcessContext.apiProcessContext.Errors)}");
                    this.output.WriteLine($"No errors found for API Host {apiProcessContext.hostId}.");
                }

                apiProcessContextCollection.End();
            }
            finally
            {
                // gottenTestRunId gets set only after successfully have gotten and validated the Test Run entity.
                if (!string.IsNullOrEmpty(gottenTestRunId))
                {
                    var response = await httpClient.DeleteItemById<TestRun>(SystemConstants.CategoryTestRunsPath, gottenTestRunId, this.output);

                    // The Delete action should success because we are validating "testRun.CompletedTime" at this.ValidateCompletedTime
                    AssertExtension.EqualResponseStatusCode(HttpStatusCode.NoContent, response);
                }
            }
        }

        /// <summary>
        /// Verifies the lode runner client status is ready.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="clientStatusId">The client status identifier.</param>
        private async Task VerifyLodeRunnerClientStatusIsReady(HttpClient httpClient, string clientStatusId)
        {
            (HttpResponseMessage httpResponseReady, Client currentClient) = await httpClient.GetClientByIdRetriesAsync(SystemConstants.CategoryClientsPath, clientStatusId, ClientStatusType.Ready, this.jsonOptions, this.output, 10, 1000);

            AssertExtension.EqualResponseStatusCode(HttpStatusCode.OK, httpResponseReady);
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
        /// <param name="maxRetries">The maximum retries.</param>
        /// <param name="timeBetweenTriesMs">The time between tries ms.</param>
        /// <returns>port number. 0 if not found.</returns>
        private async Task<int> TryParseProcessOutputAndGetAPIListeningPort(List<string> outputList, int maxRetries = 20, int timeBetweenTriesMs = 1000)
        {
            int portNumber = 0;

            var taskSource = new CancellationTokenSource();
            await Common.RunAndRetry(maxRetries, timeBetweenTriesMs, taskSource, async (int attemptCount) =>
            {
                await Task.Run(() =>
                {
                    string targetOutputLine = outputList.FirstOrDefault(s => s.Contains("Now listening on:"));
                    if (!string.IsNullOrEmpty(targetOutputLine))
                    {
                        // the line should look like "Now listening on: http://[::]:8080",  since we are splitting on ":" last string in the list will be the port either 8080 (Production) or 8081 (Development)
                        string portNumberString = targetOutputLine.Split(":").ToList().LastOrDefault();

                        if (int.TryParse(portNumberString, out portNumber))
                        {
                            this.output.WriteLine($"UTC Time:{DateTime.UtcNow}\tParsing LodeRunner API Process Output.\tPort Number Found.\tPort: '{portNumber}'\tAttempts: {attemptCount} [{timeBetweenTriesMs}ms between requests]");
                        }

                        taskSource.Cancel();
                    }
                    else
                    {
                        this.output.WriteLine($"UTC Time:{DateTime.UtcNow}\tParsing LodeRunner API Process Output.\tPort Number Not Found.\tAttempts: {attemptCount} [{timeBetweenTriesMs}ms between requests]");
                    }
                });
            });

            return portNumber;
        }

        /// <summary>
        /// Parses the process output and to get client status id.
        /// </summary>
        /// <param name="outputList">The Process outputList.</param>
        /// <param name="logName">ThelogName to identify the line be parsed.</param>
        /// <param name="marker">The marker string to identify the line be parsed.</param>
        /// <param name="fieldName">The name of the field to get the value from Json object.</param>
        /// <param name="maxRetries">The maximum retries.</param>
        /// <param name="timeBetweenTriesMs">The time between tries ms.</param>
        /// <returns>The Task with the FieldValue.</returns>
        private async Task<string> TryParseProcessOutputAndGetValueFromFieldName(List<string> outputList, string logName, string marker, string fieldName, int maxRetries = 10, int timeBetweenTriesMs = 500)
        {
            string fieldValue = null;
            var taskSource = new CancellationTokenSource();

            await Common.RunAndRetry(maxRetries, timeBetweenTriesMs, taskSource, async (int attemptCount) =>
            {
                await Task.Run(() =>
                {
                    // NOTE: we  ignore case sensitive.
                    string targetOutputLine = outputList.FirstOrDefault(s => s.Contains($"logName\":\"{logName}\"", StringComparison.InvariantCultureIgnoreCase) && s.Contains(marker, StringComparison.InvariantCultureIgnoreCase));
                    if (!string.IsNullOrEmpty(targetOutputLine))
                    {
                        Newtonsoft.Json.Linq.JObject json = Newtonsoft.Json.Linq.JObject.Parse(targetOutputLine);

                        foreach (var e in json)
                        {
                            if (e.Key.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                fieldValue = e.Value.ToString();
                                this.output.WriteLine($"UTC Time:{DateTime.UtcNow}\tParsing LodeRunner Process Output.\t'{fieldName}' Found in LogName '{logName}'.\tValue: '{fieldValue}'\tAttempts: {attemptCount} [{timeBetweenTriesMs}ms between requests]");
                                break;
                            }
                        }

                        if (fieldValue != null)
                        {
                            taskSource.Cancel();
                        }
                    }
                    else
                    {
                        this.output.WriteLine($"UTC Time:{DateTime.UtcNow}\tParsing LodeRunner Process Output.\t'{fieldName}' Not Found in LogName '{logName}'.\tAttempts: {attemptCount} [{timeBetweenTriesMs}ms between requests]");
                    }
                });
            });

            return fieldValue;
        }
    }
}
