// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using LodeRunner.API.Middleware;
using LodeRunner.API.Test.IntegrationTests.AutoMapper;
using LodeRunner.Core.Models;
using Xunit;
using Xunit.Abstractions;

namespace LodeRunner.API.Test.IntegrationTests.Controllers
{
    /// <summary>
    /// Represents TestRuns.
    /// </summary>
    public class TestRuns : IClassFixture<ApiWebApplicationFactory<Startup>>
    {
        private const string TestRunsUri = "/api/TestRuns";

        private readonly ApiWebApplicationFactory<Startup> factory;

        private readonly JsonSerializerOptions jsonOptions;

        private readonly ITestOutputHelper output;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestRuns"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="output">The output.</param>
        public TestRuns(ApiWebApplicationFactory<Startup> factory, ITestOutputHelper output)
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
        /// Determines whether this instance [can get test runs].
        /// </summary>
        /// <returns><see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("Category", "Integration")]
        public async Task CanGetTestRuns()
        {
            using var httpClient = ComponentsFactory.CreateLodeRunnerAPIHttpClient(this.factory);

            HttpResponseMessage httpResponse = await httpClient.GetAsync(TestRunsUri);

            Assert.Contains(httpResponse.StatusCode, new List<HttpStatusCode> { HttpStatusCode.OK, HttpStatusCode.NoContent });

            if (httpResponse.StatusCode == HttpStatusCode.OK)
            {
                // TODO: Separate out to test GetAllTestRuns with OK response
                var testRuns = await httpResponse.Content.ReadFromJsonAsync<List<TestRun>>(this.jsonOptions);
                this.output.WriteLine($"UTC Time:{DateTime.UtcNow}\t[{testRuns.Count}] Test Run items found.");
                Assert.NotEmpty(testRuns);
            }
            else
            {
                // TODO: Separate out to test GetAllTestRuns with No Content response
                this.output.WriteLine($"UTC Time:{DateTime.UtcNow}\t No Test Run items found, but request was successful.");
                Assert.Equal(0, httpResponse.Content.Headers.ContentLength);
            }

        }

        /// <summary>
        /// Determines whether this instance [can post test runs].
        /// </summary>
        /// <returns><see cref="Task"/> representing the asynchronous integration test.</returns>
        [Fact]
        [Trait("Category", "Integration")]
        public async Task CanCreateAndGetTestRunById()
        {
            using var httpClient = ComponentsFactory.CreateLodeRunnerAPIHttpClient(this.factory);

            HttpResponseMessage postedResponse = await httpClient.PostTestRun(TestRunsUri, this.output);
            Assert.Equal(HttpStatusCode.Created, postedResponse.StatusCode);
            var postedTestRun = await postedResponse.Content.ReadFromJsonAsync<TestRun>(this.jsonOptions);
            this.output.WriteLine($"UTC Time:{DateTime.UtcNow}\t TestRunId: [{postedTestRun.Id}] successfully created by POST method.");

            var gottenHttpResponse = await httpClient.GetAsync(TestRunsUri + "/" + postedTestRun.Id);

            Assert.Equal(HttpStatusCode.OK, gottenHttpResponse.StatusCode);
            var gottenTestRun = await gottenHttpResponse.Content.ReadFromJsonAsync<TestRun>(this.jsonOptions);
            this.output.WriteLine($"UTC Time:{DateTime.UtcNow}\t TestRunId: [{gottenTestRun.Id}] retrieved by GET method.");

            // Delete the TestRun created in this Integration Test scope
            var httpResponse = await httpClient.DeleteAsync($"{TestRunsUri}/{gottenTestRun.Id}");
            this.output.WriteLine($"UTC Time:{DateTime.UtcNow}\t TestRunId: [{gottenTestRun.Id}] deleted by DELETE method.");

            Assert.Equal(JsonSerializer.Serialize(postedTestRun), JsonSerializer.Serialize(gottenTestRun));
        }

        /// <summary>
        /// Determines whether this instance [can get clients by identifier] the specified client status identifier.
        /// </summary>
        /// <returns><see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("Category", "Integration")]
        public async Task CanPutTestRuns()
        {
            using var httpClient = ComponentsFactory.CreateLodeRunnerAPIHttpClient(this.factory);

            // Create a new TestRun
            HttpResponseMessage httpResponse = await httpClient.PostTestRun(TestRunsUri, this.output);
            var postedTestRun = await httpResponse.Content.ReadFromJsonAsync<TestRun>(this.jsonOptions);

            Assert.NotNull(postedTestRun);

            var errorsCount = ParametersValidator<TestRun>.ValidateEntityId(postedTestRun.Id).Count;

            Assert.True(errorsCount == 0, $"Local Time:{DateTime.Now}\tUnable to Post a Sample TestRun item.");

            // Generate a TestRunPayload from newly created TestRun
            var testRunPayload = postedTestRun.AutomapAndGetTestRunTestPayload();

            // Update the existing TestRun
            await httpClient.PutTestRun(testRunPayload, postedTestRun.Id, TestRunsUri, this.jsonOptions, this.output);

            // Delete the TestRun created in this Integration Test scope
            await httpClient.DeleteAsync($"{TestRunsUri}/{postedTestRun.Id}");
            this.output.WriteLine($"UTC Time:{DateTime.UtcNow}\t TestRunId: [{postedTestRun.Id}] deleted by DELETE method.");
        }

        /// <summary>
        /// Determines whether this instance [can delete test runs].
        /// </summary>
        /// <returns><see cref="Task"/> representing the asynchronous integration test.</returns>
        [Fact]
        [Trait("Category", "Integration")]
        public async Task CanDeleteTestRunById()
        {
            using var httpClient = ComponentsFactory.CreateLodeRunnerAPIHttpClient(this.factory);

            HttpResponseMessage httpResponse = await httpClient.PostTestRun(TestRunsUri, this.output);
            var testRun = await httpResponse.Content.ReadFromJsonAsync<TestRun>(this.jsonOptions);

            // Delete the TestRun created in this Integration Test scope
            await httpClient.DeleteAsync($"{TestRunsUri}/{testRun.Id}");
            this.output.WriteLine($"UTC Time:{DateTime.UtcNow}\t TestRunId: [{testRun.Id}] deleted by DELETE method.");

            var deletedTestRun = await httpClient.GetTestRunById(TestRunsUri + "/" + testRun.Id, this.jsonOptions, this.output);
            output.WriteLine($"UTC Time:{DateTime.UtcNow}\t TestRunId: [{testRun.Id}] retrieved by GET method.");
            var httpResponse = await httpClient.GetAsync(getTestRunByIdUri);
            Assert.Null(deletedTestRun);
        }
    }
}
