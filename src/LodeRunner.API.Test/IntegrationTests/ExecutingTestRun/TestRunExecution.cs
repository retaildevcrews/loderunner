﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.API.Models;
using LodeRunner.API.Test.IntegrationTests.Extensions;
using LodeRunner.Core.Models;
using Xunit;
using Xunit.Abstractions;
using CommonTest = LodeRunner.API.Test.IntegrationTests.Common;

namespace LodeRunner.API.Test.IntegrationTests.ExecutingTestRun
{
    /// <summary>
    /// Represents LoadTestConfigs.
    /// </summary>
    public class TestRunExecution : IClassFixture<ApiWebApplicationFactory<Startup>>
    {
        private const string LodeRunnerAPIAppName = "LodeRunner.API";

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

            this.jsonOptions = new()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
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
            await this.TryCreateExecuteDisposeTestRun(apiHostCount, sleepMs: 0, runLoop: false, loadClientCount: 1, async (HttpClient httpClient, TestRun postedTestRun, ProcessContext lodeRunnerAppContext, ApiProcessContextCollection apiProcessContextCollection, List<int> portList) =>
            {
                // Attempt to get TestRun for N retries or until condition has met.
                (HttpResponseMessage testRunResponse, TestRun readyTestRun) = await httpClient.GetEntityByIdRetries<TestRun>(SystemConstants.CategoryTestRunsPath, postedTestRun.Id, this.jsonOptions, this.output, this.ValidateCompletedTime, 10, apiHostCount * 3000);

                // Get LodeRunner TestRun Id when Executing
                var testRunId = await CommonTest.ParseOutputGetFieldValueAndValidateIsNotNullOrEmpty(LodeRunner.Core.SystemConstants.LodeRunnerAppName, lodeRunnerAppContext.Output, LodeRunner.Core.SystemConstants.LodeRunnerServiceLogName, LodeRunner.Core.SystemConstants.ExecutingTestRun, LodeRunner.Core.SystemConstants.TestRunIdFieldName, this.output, "Unable to get TestRunId when Executing TestRun.", 10, apiHostCount * 3000);

                // Validate that all 3 ids were logged in LodeRunner-Command output.
                this.output.WriteLine($"Validating {LodeRunner.Core.SystemConstants.ClientStatusIdFieldName}, {LodeRunner.Core.SystemConstants.LoadClientIdFieldName}  and {LodeRunner.Core.SystemConstants.TestRunIdFieldName} for LodeRunner-Command Log");
                string lodeRunnerCmdOutputMarker = string.Format(SystemConstants.BaseUriLocalHostPort, portList[0]);

                var clientStatusId = await CommonTest.ParseOutputGetFieldValueAndValidateIsNotNullOrEmpty(LodeRunner.Core.SystemConstants.LodeRunnerAppName, lodeRunnerAppContext.Output, LodeRunner.Core.SystemConstants.LoadTestRequestLogName, lodeRunnerCmdOutputMarker, LodeRunner.Core.SystemConstants.ClientStatusIdFieldName, this.output);

                var loadClientId = await CommonTest.ParseOutputGetFieldValueAndValidateIsNotNullOrEmpty(LodeRunner.Core.SystemConstants.LodeRunnerAppName, lodeRunnerAppContext.Output, LodeRunner.Core.SystemConstants.LoadTestRequestLogName, lodeRunnerCmdOutputMarker, LodeRunner.Core.SystemConstants.LoadClientIdFieldName, this.output);

                testRunId = await CommonTest.ParseOutputGetFieldValueAndValidateIsNotNullOrEmpty(LodeRunner.Core.SystemConstants.LodeRunnerAppName, lodeRunnerAppContext.Output, LodeRunner.Core.SystemConstants.LoadTestRequestLogName, lodeRunnerCmdOutputMarker, LodeRunner.Core.SystemConstants.TestRunIdFieldName, this.output);

                // Validate that TraceId and SpanId were logged in LodeRunner-Command output.
                this.output.WriteLine($"Validating {LodeRunner.Core.SystemConstants.B3TraceIdFieldName} and {LodeRunner.Core.SystemConstants.B3SpanIdFieldName} for LodeRunner-Command Log");

                var traceId = await CommonTest.ParseOutputGetFieldValueAndValidateIsNotNullOrEmpty(LodeRunner.Core.SystemConstants.LodeRunnerAppName, lodeRunnerAppContext.Output, LodeRunner.Core.SystemConstants.LoadTestRequestLogName, lodeRunnerCmdOutputMarker, LodeRunner.Core.SystemConstants.B3TraceIdFieldName, this.output);

                var spanId = await CommonTest.ParseOutputGetFieldValueAndValidateIsNotNullOrEmpty(LodeRunner.Core.SystemConstants.LodeRunnerAppName, lodeRunnerAppContext.Output, LodeRunner.Core.SystemConstants.LoadTestRequestLogName, lodeRunnerCmdOutputMarker, LodeRunner.Core.SystemConstants.B3SpanIdFieldName, this.output);

                // Validate traceId and SpanId were logged in LodeRunner.API log
                foreach (var (hostId, portNumber, apiProcessContext) in apiProcessContextCollection)
                {
                    this.output.WriteLine($"Validating {LodeRunner.Core.SystemConstants.B3TraceIdFieldName} and {LodeRunner.Core.SystemConstants.B3SpanIdFieldName} for LodeRunner API Log for Host {hostId}.");

                    string lodeRunnerAPIOutputMarker = $"localhost:{portNumber}";

                    traceId = await CommonTest.ParseOutputGetFieldValueAndValidateIsNotNullOrEmpty(LodeRunnerAPIAppName, apiProcessContext.Output, LodeRunner.Core.SystemConstants.LodeRunnerAPIRequestLogName, lodeRunnerAPIOutputMarker, LodeRunner.Core.SystemConstants.B3TraceIdFieldName, this.output);

                    spanId = await CommonTest.ParseOutputGetFieldValueAndValidateIsNotNullOrEmpty(LodeRunnerAPIAppName, apiProcessContext.Output, LodeRunner.Core.SystemConstants.LodeRunnerAPIRequestLogName, lodeRunnerAPIOutputMarker, LodeRunner.Core.SystemConstants.B3SpanIdFieldName, this.output);
                }

                // Validate results
                ValidateTestRunResults(readyTestRun, testRunResponse);

                var clientResult = readyTestRun.ClientResults[0];

                Assert.True(clientResult.TotalRequests > 0, $"Total requests should be grater that zero.");
                Assert.True(clientResult.TotalRequests == clientResult.FailedRequests + clientResult.SuccessfulRequests, $"TotalRequests {clientResult.TotalRequests} does not match expected value {clientResult.FailedRequests + clientResult.SuccessfulRequests}");

                this.output.WriteLine($"UTC Time:{DateTime.UtcNow}\tTestRun passed validation.");
            });
        }

        /// <summary>
        /// Determines whether this instance [can create and execute test run with a given number of API hosts].
        /// </summary>
        /// <param name="apiHostCount">The number API hosts to utilized.</param>
        /// <param name="sleepMs">The sleep time between requests in ms.</param>
        /// <param name="runLoop">Detemines if should run in loop.</param>
        /// <param name="expectedCancellationErrorMessage">The Expected Cancellation Error Message.</param>
        /// <returns><see cref="Task"/> representing the asynchronous integration test.</returns>
        [Trait("Category", "Integration")]
        [Theory]
        [InlineData(1, 5000, false, LodeRunner.Core.SystemConstants.TestRunExecutionStoppedMessage)]
        [InlineData(1, 5000, true, LodeRunner.Core.SystemConstants.OperationCanceledException)]
        public async Task CanCreateExecuteAndStopTestRun(int apiHostCount, int sleepMs, bool runLoop, string expectedCancellationErrorMessage)
        {
            await this.TryCreateExecuteDisposeTestRun(apiHostCount, sleepMs, runLoop, loadClientCount: 1, async (HttpClient httpClient, TestRun postedTestRun, ProcessContext lodeRunnerAppContext, ApiProcessContextCollection apiProcessContextCollection, List<int> portList) =>
            {
                // Async method to set HardStop to true.
                this.output.WriteLine($"UTC Time:{DateTime.UtcNow}\tRequesting Test Run Cancellation...");

                await this.SetHardStopTrueRetryAsync(postedTestRun.Id);

                // Attempt to get TestRun for N retries or until condition has met.
                (HttpResponseMessage testRunResponse, TestRun readyTestRun) = await httpClient.GetEntityByIdRetries<TestRun>(SystemConstants.CategoryTestRunsPath, postedTestRun.Id, this.jsonOptions, this.output, this.ValidateHardStopTime, 10, apiHostCount * 3000);

                // Get LodeRunner TestRun Id when Executing
                var testRunId = await CommonTest.ParseOutputGetFieldValueAndValidateIsNotNullOrEmpty(LodeRunner.Core.SystemConstants.LodeRunnerAppName, lodeRunnerAppContext.Output, LodeRunner.Core.SystemConstants.LodeRunnerServiceLogName, LodeRunner.Core.SystemConstants.ExecutingTestRun, LodeRunner.Core.SystemConstants.TestRunIdFieldName, this.output, "Unable to get TestRunId when Executing TestRun.", 10, apiHostCount * 3000);

                this.output.WriteLine($"UTC Time:{DateTime.UtcNow}\tValidating Test Run Request Cancellation results ...");

                // Validate that Request for HardStop was received was loggged in LodeRunner-Command output.
                var testRunCancellationRequestReceivedMessage = await CommonTest.ParseOutputGetFieldValueAndValidateIsNotNullOrEmpty(LodeRunner.Core.SystemConstants.LodeRunnerAppName, lodeRunnerAppContext.Output, LodeRunner.Core.SystemConstants.LodeRunnerServiceLogName, LodeRunner.Core.SystemConstants.TestRunCancellationRequestReceivedMessage, "message", this.output, "Unable to get TestRun Cancellation Request Received Message from LodeRunner-Command output");

                // Validate that the cancellation message match the current TestRunId
                Assert.True(testRunCancellationRequestReceivedMessage.Contains(testRunId), "Unable to match TestRunId for Cancellation Request Message in LodeRunner-Command output");

                (HttpResponseMessage stoppedTestRunResponse, TestRun stoppedTestRun) = await httpClient.GetEntityByIdRetries<TestRun>(SystemConstants.CategoryTestRunsPath, postedTestRun.Id, this.jsonOptions, this.output, this.ValidateHardStopTime, 10, apiHostCount * 500);

                // Validate Hard Stop completed message was loggged in LodeRunner-Command output.
                var testRunHardStopCommpletedMessage = await CommonTest.ParseOutputGetFieldValueAndValidateIsNotNullOrEmpty(LodeRunner.Core.SystemConstants.LodeRunnerAppName, lodeRunnerAppContext.Output, LodeRunner.Core.SystemConstants.LodeRunnerServiceLogName, LodeRunner.Core.SystemConstants.TestRunHardStopCompletedMessage, "message", this.output, "Unable to get TestRun Hard Stop Completed Message from LodeRunner-Command output", 10, readyTestRun.LoadClients.Count * 1000);

                // Validate that the cancellation message match the current TestRunId
                Assert.True(testRunHardStopCommpletedMessage.Contains(testRunId), "Unable to match TestRunId for Hard Stop Completed in LodeRunner-Command output");

                // Validate results
                ValidateTestRunResults(readyTestRun, testRunResponse);

                // Validate Hard Stop values in TestRun item.
                Assert.True(stoppedTestRun.HardStop && stoppedTestRun.HardStopTime != null, $"TestRun was requested to be cancelled, however HardStop is set to '{stoppedTestRun.HardStop}' and HardStopTime is set to '{stoppedTestRun.HardStopTime}'.");

                var clientResult = readyTestRun.ClientResults[0];

                Assert.True(clientResult.ErrorMessage == expectedCancellationErrorMessage, $"Actual error message found '{clientResult.ErrorMessage}', instead of expected message '{expectedCancellationErrorMessage}'");
                Assert.True(clientResult.TotalRequests == 0, "Total Requests should be equals to 0.");
                Assert.True(clientResult.FailedRequests == 0, "Failed Requests should be equals to 0.");
                Assert.True(clientResult.SuccessfulRequests == 0, "Successful Requests should be equals to 0.");

                this.output.WriteLine($"UTC Time:{DateTime.UtcNow}\tValidation for TestRun Cancellation passed.");
            });
        }

        /// <summary>
        /// Validate TestRun Results.
        /// </summary>
        /// <param name="readyTestRun">The TestRun to validate.</param>
        /// <param name="testRunResponse">The HttpResponseMessage.</param>
        private static void ValidateTestRunResults(TestRun readyTestRun, HttpResponseMessage testRunResponse)
        {
            int expectedLoadClientCount = 1;
            AssertExtension.EqualResponseStatusCode(HttpStatusCode.OK, testRunResponse);

            Assert.True(readyTestRun.CompletedTime != null, "CompletedTime is null.");
            Assert.True(readyTestRun.LoadClients.Count == expectedLoadClientCount, $"LoadClients.Count do not match the expected value [{expectedLoadClientCount}]");
            Assert.True(readyTestRun.ClientResults.Count == readyTestRun.LoadClients.Count, "ClientResults.Count do not match LoadClients.Count");
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
        private async Task<(bool Result, string FieldName, string ConditionalValue)> ValidateCompletedTime(TestRun testRun)
        {
            return await Task.Run(() =>
            {
                return (testRun.CompletedTime != null, "CompletedTime", testRun.CompletedTime == null ? "null" : testRun.CompletedTime.ToString());
            });
        }

        /// <summary>
        /// Validates the hardstop time.
        /// </summary>
        /// <param name="testRun">The test run.</param>
        /// <returns>The Task.</returns>
        private async Task<(bool Result, string FieldName, string ConditionalValue)> ValidateHardStopTime(TestRun testRun)
        {
            return await Task.Run(() =>
            {
                return (testRun.HardStopTime != null, "HardStopTime", testRun.HardStopTime == null ? "null" : testRun.HardStopTime.ToString());
            });
        }

        /// <summary>
        /// Set to True Hard Stop for the given Test Run Id.
        /// </summary>
        /// <param name="testRunId">The test run id.</param>
        /// <returns>The Task.</returns>
        private async Task SetHardStopTrueRetryAsync(string testRunId, int maxRetries = 10, int timeBetweenRequestsMs = 1000)
        {
            await Task.Run(async () =>
            {
                var taskSource = new CancellationTokenSource();

                await LodeRunner.Core.Common.RunAndRetry(maxRetries, timeBetweenRequestsMs, taskSource, async (int attemptCount) =>
                {
                    var testRunSvc = ComponentsFactory.GetTestRunService(this.factory);

                    // get current TestRun document
                    var testRun = await testRunSvc.Get(testRunId);

                    if (!testRun.HardStop && testRun.HardStopTime == null)
                    {
                        testRun.HardStop = true;
                        await testRunSvc.Post(testRun, CancellationToken.None);

                        this.output.WriteLine($"UTC Time:{DateTime.UtcNow}\tTestRun HardStop field set to: '{testRun.HardStop}'\tAttempts: {attemptCount} [{timeBetweenRequestsMs}ms between requests]");

                        taskSource.Cancel();
                    }

                    this.output.WriteLine($"UTC Time:{DateTime.UtcNow}\tTestRun HardStop field was already set to: '{testRun.HardStop}'\tAttempts: {attemptCount} [{timeBetweenRequestsMs}ms between requests]");
                });
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
            await LodeRunner.Core.Common.RunAndRetry(maxRetries, timeBetweenTriesMs, taskSource, async (int attemptCount) =>
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
        /// Reusable Try-Create-Execute-Dispose Common TestRun block to encapsulate specific testRunTaskToExecute Implementation
        /// while handling common code blocks such as LodeRunner and LodeRunner API instantiation.
        /// </summary>
        /// <param name="apiHostCount">The number API hosts to utilized.</param>
        /// <param name="sleepMs">The sleep time between requests in ms.</param>
        /// <param name="runLoop">Detemines if should run in loop.</param>
        /// <param name="loadClientCount">The number of loadClients to create for the TestRun.</param>
        /// <param name="concreteTestRunTaskToExecute">The concrete task to be executed in this block.</param>
        /// <returns>A task for the in-line code block.</returns>
        private async Task TryCreateExecuteDisposeTestRun(int apiHostCount, int sleepMs, bool runLoop, int loadClientCount, Func<HttpClient, TestRun, ProcessContext, ApiProcessContextCollection, List<int>, Task> concreteTestRunTaskToExecute)
        {
            using var httpClient = ComponentsFactory.CreateLodeRunnerAPIHttpClient(this.factory);
            // TODO: create a LRProcessContextCollection similar to ApiProcessContextCollection , so we can handle multiple loadClients

            // Execute dotnet run against LodeRunner project in Client Mode
            string secretsVolume = "secrets".GetSecretVolume();
            using var lodeRunnerAppContext = new ProcessContext(
                new ProcessContextParams()
                {
                    ProjectBasePath = "LodeRunner/LodeRunner.csproj",
                    ProjectArgs = $"--mode Client --secrets-volume {secretsVolume}",
                    ProjectBaseParentDirectoryName = "src",
                },
                this.output);

            using var apiProcessContextCollection = new ApiProcessContextCollection(apiHostCount, secretsVolume, this.output);

            string gottenTestRunId = string.Empty;

            try
            {
                this.output.WriteLine($"Starting LodeRunner Application (client mode)");

                Assert.True(lodeRunnerAppContext.Start(), "Unable to start LodeRunner App Context.");

                // Get LodeRunner Client Status Id when Initializing Client
                var clientStatusId = await CommonTest.ParseOutputGetFieldValueAndValidateIsNotNullOrEmpty(LodeRunner.Core.SystemConstants.LodeRunnerAppName, lodeRunnerAppContext.Output, LodeRunner.Core.SystemConstants.LodeRunnerServiceLogName, LodeRunner.Core.SystemConstants.InitializingClient, LodeRunner.Core.SystemConstants.ClientStatusIdFieldName, this.output, "Unable to get ClientStatusId when Initializing Client.");

                // Get LodeRunner LoadClient Id when Initializing Client
                var loadClientId = await CommonTest.ParseOutputGetFieldValueAndValidateIsNotNullOrEmpty(LodeRunner.Core.SystemConstants.LodeRunnerAppName, lodeRunnerAppContext.Output, LodeRunner.Core.SystemConstants.LodeRunnerServiceLogName, LodeRunner.Core.SystemConstants.InitializingClient, LodeRunner.Core.SystemConstants.LoadClientIdFieldName, this.output, "Unable to get loadClientId when Initializing Client", 10, apiHostCount * 2000);

                // We should not have any error at time we are going to Verify Id
                Assert.True(lodeRunnerAppContext.Errors.Count == 0, $"Errors found in LodeRunner Output.{Environment.NewLine}{string.Join(",", lodeRunnerAppContext.Errors)}");

                Assert.True(apiProcessContextCollection.Start(this.factory.GetNextAvailablePort), $"Api ProcessContext Collection.");

                List<int> portList = new();

                foreach (var (hostId, portNumber, apiProcessContext) in apiProcessContextCollection)
                {
                    this.output.WriteLine($"Starting LodeRunner API for Host {hostId}.");

                    Assert.True(apiProcessContext.Started, $"Unable to start LodeRunner API Context for Host {hostId}.");

                    Assert.True(apiProcessContext.Errors.Count == 0, $"Errors found in LodeRunner API - Host {hostId} Output.{Environment.NewLine}{string.Join(",", apiProcessContext.Errors)}");

                    int apiListeningOnPort = await this.TryParseProcessOutputAndGetAPIListeningPort(apiProcessContext.Output);

                    Assert.True(apiListeningOnPort == portNumber, "Unable to get Port Number");

                    portList.Add(apiListeningOnPort);
                }

                // Verify that clientStatusId exist is Database.
                await this.VerifyLodeRunnerClientStatusIsReady(httpClient, clientStatusId);

                // Create Test Run
                TestRunPayload testRunPayload = new();

                string testRunName = $"Sample TestRun - IntegrationTesting-{nameof(this.CanCreateAndExecuteTestRun)}-{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}";

                // NOTE: InData parameters detemine that we create a long running test for each run mode (run-loop and run-once)
                testRunPayload.SetMockDataToLoadTestLodeRunnerApi(testRunName, loadClientId, loadClientCount, portList, sleepMs, runLoop);

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
                var testRunId = await CommonTest.ParseOutputGetFieldValueAndValidateIsNotNullOrEmpty(LodeRunner.Core.SystemConstants.LodeRunnerAppName, lodeRunnerAppContext.Output, LodeRunner.Core.SystemConstants.LodeRunnerServiceLogName, LodeRunner.Core.SystemConstants.ReceivedNewTestRun, LodeRunner.Core.SystemConstants.TestRunIdFieldName, this.output, "Unable to get TestRunId when Received TestRun", 10, apiHostCount * 3000);

                // Execute Task for Concrete TestRun Implementation.
                await concreteTestRunTaskToExecute(httpClient, postedTestRun, lodeRunnerAppContext, apiProcessContextCollection, portList);

                // Verify that ClientStatus is Ready again.
                await this.VerifyLodeRunnerClientStatusIsReady(httpClient, clientStatusId);

                // End LodeRunner Context.
                lodeRunnerAppContext.End();
                this.output.WriteLine($"Stopping LodeRunner Application (client mode) [ClientStatusId: {clientStatusId}]");

                foreach (var (hostId, portNumber, apiProcessContext) in apiProcessContextCollection)
                {
                    Assert.True(apiProcessContext.Errors.Count == 0, $"Errors found in LodeRunner API - Host {hostId} Output.{Environment.NewLine}{string.Join(",", apiProcessContext.Errors)}");
                    this.output.WriteLine($"No errors found for API Host {hostId}.");
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
    }
}
