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

            // string args = $"run --project ../../../../LodeRunner/LodeRunner.csproj --mode Command -s http://localhost:8081 --files LodeRunner.Api-benchmark.json --run-loop true --duration 10";
            string args = $"run --project ../../../../LodeRunner/LodeRunner.csproj --mode Client --secrets-volume secrets";
            using var lodeRunnerAppContext = new ProcessContext(cmdLine, args, this.output);

            try
            {

                if (lodeRunnerAppContext.Start())
                {
                    string clientStatusId = null;
                    while (lodeRunnerAppContext.IsRunning)
                    {
                        if (string.IsNullOrEmpty(clientStatusId))
                        {
                            clientStatusId = this.GetClienStatusId(lodeRunnerAppContext.Output);
                        }
                        else
                        {
                            // Verify clientStatusId.
                            (HttpStatusCode readyStatusCode, Client readyClient) = await httpClient.GetClientByIdRetriesAsync(Clients.ClientsUri, clientStatusId, ClientStatusType.Ready, this.jsonOptions, this.output);

                            Assert.Equal(HttpStatusCode.OK, readyStatusCode);
                            Assert.NotNull(readyClient);
                            Assert.Equal(clientStatusId, readyClient.ClientStatusId);


                            // TODO:

                            // Create Test Run
                            TestRunPayload testRunPayload = new ();

                            testRunPayload.SetMockDataForLoadRunnerApi($"Sample TestRun - IntegrationTesting-{nameof(this.CanCreateAndExecuteTestRun)}-{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}");

                            HttpResponseMessage postedResponse = await httpClient.PostEntity<TestRun, TestRunPayload>(testRunPayload, TestRunsUri, this.output);
                            Assert.Equal(HttpStatusCode.Created, postedResponse.StatusCode);

                            var postedTestRun = await postedResponse.Content.ReadFromJsonAsync<TestRun>(this.jsonOptions);
                            var gottenHttpResponse = await httpClient.GetItemById<TestRun>(TestRunsUri, postedTestRun.Id, this.output);

                            Assert.Equal(HttpStatusCode.OK, gottenHttpResponse.StatusCode);
                            var gottenTestRun = await gottenHttpResponse.Content.ReadFromJsonAsync<TestRun>(this.jsonOptions);

                            Assert.Equal(JsonSerializer.Serialize(postedTestRun), JsonSerializer.Serialize(gottenTestRun));


                            // TODO:
                            // Wait for test execution to complete. what is the feedback from LodeRunner utilize   //var output = lodeRunnerAppContext.Output;

                            // Check expected results

                            //setup the mechanism to look for the TestRun to have results and be complete or fail(which you can test by manually updating the LoadResults

                            // then terminate LodeRunner Context.
                            lodeRunnerAppContext.End();

                        }
                    }

                    //var errors = lodeRunnerAppContext.Errors;

                    this.output.WriteLine($"Stopping LodeRunner Application (client mode) [ClientStatusId: {clientStatusId}]");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                lodeRunnerAppContext?.Dispose();
            }

            // TODO:  Delete the TestRun ? <<<<<<<<<<<<  this could be a different test since we need to check for Completed DateTime
            //await httpClient.DeleteItemById<TestRun>(TestRunsUri, gottenTestRun.Id, this.output);
        }

        private string GetClienStatusId(List<string> output)
        {
            string targetOutputLine = output.FirstOrDefault(s => s.Contains(LodeRunner.Core.SystemConstants.ClientReady));
            if (!string.IsNullOrEmpty(targetOutputLine))
            {
                return this.GetSubstringByString("(", ")", targetOutputLine);
            }

            return null;
        }

        private string GetSubstringByString(string openString, string closingString, string baseString)
        {
            return baseString.Substring(baseString.IndexOf(openString) + openString.Length, baseString.IndexOf(closingString) - baseString.IndexOf(openString) - openString.Length);
        }
    }
}
