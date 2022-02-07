// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using LodeRunner.API.Models;
using LodeRunner.API.Test.IntegrationTests.Controllers;
using LodeRunner.Core.Automapper;
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


            //TODO : Start a LodeRunner Service using Process Start ?

            // Create and Start LoadRunner Service - Client Mode
            //using var l8rService = await ComponentsFactory.CreateAndStartLodeRunnerServiceInstance(nameof(this.CanCreateAndExecuteTestRun));
            //string clientStatusId = l8rService.ClientStatusId;
            //this.output.WriteLine($"Started LodeRunner (client mode) [ClientStatusId: {clientStatusId}]");

            //Assert.False(string.IsNullOrEmpty(clientStatusId), "Unable to retrieve ClientStatusId from LodeRunner (client mode) service.");

            //(HttpStatusCode readyStatusCode, Client readyClient) = await httpClient.GetClientByIdRetriesAsync(Clients.ClientsUri, clientStatusId, ClientStatusType.Ready, this.jsonOptions, this.output);

            //Assert.Equal(HttpStatusCode.OK, readyStatusCode);
            //Assert.NotNull(readyClient);
            //Assert.Equal(clientStatusId, readyClient.ClientStatusId);

            //TODO: Build LoadRunner and get location


            using var context = new ProcessContext("process info");

            try
            {
                context.Start();

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

                // Wait for test execution to complete
                // Check expected results


                //l8rService.StopService();
                //this.output.WriteLine($"Stopping LodeRunner (client mode) [ClientStatusId: {clientStatusId}]");

                context.End();
            }
            finally
            {
                context?.Dispose();
            }

            // TODO:  Delete the TestRun ? <<<<<<<<<<<<  this could be a different test since we need to check for Completed DateTime
            //await httpClient.DeleteItemById<TestRun>(TestRunsUri, gottenTestRun.Id, this.output);
        }
    }
}
