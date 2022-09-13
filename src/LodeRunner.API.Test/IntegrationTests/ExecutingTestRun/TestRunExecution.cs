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
    /// Represents TestRunExecution.
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
            using var httpClient = ComponentsFactory.CreateLodeRunnerAPIHttpClient(this.factory);

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

                string lodeRunnerServiceLogName = "LodeRunner.Services.LodeRunnerService";

                // Get LodeRunner Client Status Id.
                string clientStatusId = await LogOutputExtension.TryParseProcessOutputAndGetValueFromFieldName(lodeRunnerAppContext.Output, lodeRunnerServiceLogName, LodeRunner.Core.SystemConstants.InitializingClient, LodeRunner.Core.SystemConstants.ClientStatusIdFieldName, this.output);
                Assert.False(string.IsNullOrEmpty(clientStatusId), "Unable to get ClientStatusId when Initializing Client.");

                // Get LodeRunner LoadClient Id.
                string loadClientId = await LogOutputExtension.TryParseProcessOutputAndGetValueFromFieldName(lodeRunnerAppContext.Output, lodeRunnerServiceLogName, LodeRunner.Core.SystemConstants.InitializingClient, LodeRunner.Core.SystemConstants.LoadClientIdFieldName, this.output, 10, apiHostCount * 2000);
                Assert.False(string.IsNullOrEmpty(loadClientId), "Unable to get loadClientId");

                // We should not have any error at time we are going to Verify Id
                Assert.True(lodeRunnerAppContext.Errors.Count == 0, $"Errors found in LodeRunner Output.{Environment.NewLine}{string.Join(",", lodeRunnerAppContext.Errors)}");

                Assert.True(apiProcessContextCollection.Start(this.factory.GetNextAvailablePort), $"Api ProcessContext Collection.");

                List<int> portList = new();

                foreach (var (hostId, portNumber, apiProcessContext) in apiProcessContextCollection)
                {
                    this.output.WriteLine($"Starting LodeRunner API for Host {hostId}.");

                    Assert.True(apiProcessContext.Started, $"Unable to start LodeRunner API Context for Host {hostId}.");

                    Assert.True(apiProcessContext.Errors.Count == 0, $"Errors found in LodeRunner API - Host {hostId} Output.{Environment.NewLine}{string.Join(",", apiProcessContext.Errors)}");

                    int apiListeningOnPort = await LogOutputExtension.TryParseProcessOutputAndGetAPIListeningPort(apiProcessContext.Output, this.output);

                    Assert.True(apiListeningOnPort == portNumber, "Unable to get Port Number");

                    portList.Add(apiListeningOnPort);
                }

                // Verify that clientStatusId exist is Database.
                await this.VerifyLodeRunnerClientStatusIsReady(httpClient, clientStatusId);

                // Create Test Run
                TestRunPayload testRunPayload = new();

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
                string testRunId = await LogOutputExtension.TryParseProcessOutputAndGetValueFromFieldName(lodeRunnerAppContext.Output, lodeRunnerServiceLogName, LodeRunner.Core.SystemConstants.ReceivedNewTestRun, LodeRunner.Core.SystemConstants.TestRunIdFieldName, this.output, 10, apiHostCount * 3000);
                Assert.False(string.IsNullOrEmpty(testRunId), "Unable to get TestRunId when Received TestRun");

                // Attempt to get TestRun for N retries or until condition has met.
                (HttpResponseMessage testRunResponse, TestRun readyTestRun) = await httpClient.GetEntityByIdRetries<TestRun>(SystemConstants.CategoryTestRunsPath, postedTestRun.Id, this.jsonOptions, this.output, this.ValidateCompletedTime, 10, apiHostCount * 3000);

                // Get LodeRunner TestRun Id when Executing
                testRunId = await LogOutputExtension.TryParseProcessOutputAndGetValueFromFieldName(lodeRunnerAppContext.Output, lodeRunnerServiceLogName, LodeRunner.Core.SystemConstants.ExecutingTestRun, LodeRunner.Core.SystemConstants.TestRunIdFieldName, this.output, 10, apiHostCount * 3000);
                Assert.False(string.IsNullOrEmpty(testRunId), "Unable to get TestRunId when Executing TestRun.");

                // Validate that all 3 ids were logged in LodeRunner-Command output.
                this.output.WriteLine($"Validating {LodeRunner.Core.SystemConstants.ClientStatusIdFieldName}, {LodeRunner.Core.SystemConstants.LoadClientIdFieldName}  and {LodeRunner.Core.SystemConstants.TestRunIdFieldName} for LodeRunner-Command Log");
                string lodeRunnerCmdOutputMarker = string.Format(SystemConstants.BaseUriLocalHostPort, portList[0]);
                clientStatusId = await LogOutputExtension.TryParseProcessOutputAndGetValueFromFieldName(lodeRunnerAppContext.Output, LodeRunner.Core.SystemConstants.LoadTestRequestLogName, lodeRunnerCmdOutputMarker, LodeRunner.Core.SystemConstants.ClientStatusIdFieldName, this.output, 10, 500);
                Assert.False(string.IsNullOrEmpty(clientStatusId), "Unable to get ClientStatusId from LodeRunner-Command output");

                loadClientId = await LogOutputExtension.TryParseProcessOutputAndGetValueFromFieldName(lodeRunnerAppContext.Output, LodeRunner.Core.SystemConstants.LoadTestRequestLogName, lodeRunnerCmdOutputMarker, LodeRunner.Core.SystemConstants.LoadClientIdFieldName, this.output, 10, 500);
                Assert.False(string.IsNullOrEmpty(loadClientId), "Unable to get LoadClientId from LodeRunner-Command output");

                testRunId = await LogOutputExtension.TryParseProcessOutputAndGetValueFromFieldName(lodeRunnerAppContext.Output, LodeRunner.Core.SystemConstants.LoadTestRequestLogName, lodeRunnerCmdOutputMarker, LodeRunner.Core.SystemConstants.TestRunIdFieldName, this.output, 10, 500);
                Assert.False(string.IsNullOrEmpty(testRunId), "Unable to get TestRunId from LodeRunner-Command output");

                // Validate that TraceId and SpanId were logged in LodeRunner-Command output.
                this.output.WriteLine($"Validating {LodeRunner.Core.SystemConstants.B3TraceIdFieldName} and {LodeRunner.Core.SystemConstants.B3SpanIdFieldName} for LodeRunner-Command Log");
                var traceId = await LogOutputExtension.TryParseProcessOutputAndGetValueFromFieldName(lodeRunnerAppContext.Output, LodeRunner.Core.SystemConstants.LoadTestRequestLogName, lodeRunnerCmdOutputMarker, LodeRunner.Core.SystemConstants.B3TraceIdFieldName, this.output, 10, 500);
                Assert.False(string.IsNullOrEmpty(traceId), "Unable to get B3TraceId from LodeRunner-Command output");

                var spanId = await LogOutputExtension.TryParseProcessOutputAndGetValueFromFieldName(lodeRunnerAppContext.Output, LodeRunner.Core.SystemConstants.LoadTestRequestLogName, lodeRunnerCmdOutputMarker, LodeRunner.Core.SystemConstants.B3SpanIdFieldName, this.output, 10, 500);
                Assert.False(string.IsNullOrEmpty(spanId), "Unable to get B3SpanId from LodeRunner-Command output");

                // Validate traceId and SpanId were logged in LodeRunner.API log
                foreach (var (hostId, portNumber, apiProcessContext) in apiProcessContextCollection)
                {
                    this.output.WriteLine($"Validating {LodeRunner.Core.SystemConstants.B3TraceIdFieldName} and {LodeRunner.Core.SystemConstants.B3SpanIdFieldName} for LodeRunner API Log for Host {hostId}.");

                    string lodeRunnerAPIOutputMarker = $"localhost:{portNumber}";

                    traceId = await LogOutputExtension.TryParseProcessOutputAndGetValueFromFieldName(apiProcessContext.Output, LodeRunner.Core.SystemConstants.LodeRunnerAPIRequestLogName, lodeRunnerAPIOutputMarker, LodeRunner.Core.SystemConstants.B3TraceIdFieldName, this.output, 10, 500);
                    Assert.False(string.IsNullOrEmpty(traceId), "Unable to get B3TraceId from LodeRunner.API output");

                    spanId = await LogOutputExtension.TryParseProcessOutputAndGetValueFromFieldName(apiProcessContext.Output, LodeRunner.Core.SystemConstants.LodeRunnerAPIRequestLogName, lodeRunnerAPIOutputMarker, LodeRunner.Core.SystemConstants.B3SpanIdFieldName, this.output, 10, 500);
                    Assert.False(string.IsNullOrEmpty(spanId), "Unable to get B3SpanId from LodeRunner.API output");

                    var parentSpanId = await LogOutputExtension.TryParseProcessOutputAndGetValueFromFieldName(apiProcessContext.Output, LodeRunner.Core.SystemConstants.LodeRunnerAPIRequestLogName, lodeRunnerAPIOutputMarker, LodeRunner.Core.SystemConstants.B3ParentSpanIdFieldName, this.output, 10, 500);
                    Assert.False(string.IsNullOrEmpty(parentSpanId), "Unable to get B3ParentSpanId from LodeRunner.API output");
                }

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
    }
}
