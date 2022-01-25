﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using LodeRunner.Core.Models;
using Microsoft.AspNetCore.Mvc;
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

            HttpResponseMessage httpResponse = await httpClient.GetTestRuns(TestRunsUri, this.output);
            Assert.Contains(httpResponse.StatusCode, new List<HttpStatusCode> { HttpStatusCode.OK, HttpStatusCode.NoContent });

            if (httpResponse.StatusCode == HttpStatusCode.OK)
            {
                // TODO: Separate out to test GetAllTestRuns with OK response
                var testRuns = await httpResponse.Content.ReadFromJsonAsync<List<TestRun>>(this.jsonOptions);
                Assert.NotEmpty(testRuns);
            }
            else
            {
                // TODO: Separate out to test GetAllTestRuns with No Content response
                Assert.Equal(0, httpResponse.Content.Headers.ContentLength);
            }
        }

        /// <summary>
        /// Determines whether this instance [can post a test run and get a test run by ID].
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
            var gottenHttpResponse = await httpClient.GetTestRunById(TestRunsUri, postedTestRun.Id, this.output);

            Assert.Equal(HttpStatusCode.OK, gottenHttpResponse.StatusCode);
            var gottenTestRun = await gottenHttpResponse.Content.ReadFromJsonAsync<TestRun>(this.jsonOptions);

            Assert.Equal(JsonSerializer.Serialize(postedTestRun), JsonSerializer.Serialize(gottenTestRun));

            // Delete the TestRun created in this Integration Test scope
            await httpClient.DeleteTestRunById(TestRunsUri, gottenTestRun.Id, this.output);
        }

        /// <summary>
        /// Determines whether this instance [can update test run by identifier].
        /// </summary>
        /// <returns><see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("Category", "Integration")]
        public async Task CanPutTestRuns()
        {
            using var httpClient = ComponentsFactory.CreateLodeRunnerAPIHttpClient(this.factory);

            // Create a new TestRun
            HttpResponseMessage postResponse = await httpClient.PostTestRun(TestRunsUri, this.output);
            var postedTestRun = await postResponse.Content.ReadFromJsonAsync<TestRun>(this.jsonOptions);

            TestRunPayload testRunPayload = new ();
            testRunPayload.Name = $"Updated TestRun - IntegrationTesting-{nameof(this.CanPutTestRuns)}-{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}";
            testRunPayload.CreatedTime = postedTestRun.CreatedTime;
            testRunPayload.StartTime = postedTestRun.StartTime;
            testRunPayload.LoadTestConfig = postedTestRun.LoadTestConfig;
            testRunPayload.LoadClients = postedTestRun.LoadClients;

            // Update TestRun
            var puttedResponse = await httpClient.PutTestRunById(TestRunsUri, postedTestRun.Id, testRunPayload, this.output);

            Assert.Equal(HttpStatusCode.NoContent, puttedResponse.StatusCode);

            var gottenResponse = await httpClient.GetTestRunById(TestRunsUri, postedTestRun.Id, this.output);
            var gottenTestRun = await gottenResponse.Content.ReadFromJsonAsync<TestRun>(this.jsonOptions);
            postedTestRun.Name = testRunPayload.Name;

            Assert.Equal(JsonSerializer.Serialize(postedTestRun), JsonSerializer.Serialize(gottenTestRun));
        }

        /// <summary>
        /// Determines whether this instance [can delete a test run by ID].
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
            var deletedResponse = await httpClient.DeleteTestRunById(TestRunsUri, testRun.Id, this.output);
            Assert.Equal(HttpStatusCode.NoContent, deletedResponse.StatusCode);

            Assert.Equal(0, deletedResponse.Content.Headers.ContentLength);

            var gottenHttpResponse = await httpClient.GetTestRunById(TestRunsUri, testRun.Id, this.output);
            Assert.Equal(HttpStatusCode.NotFound, gottenHttpResponse.StatusCode);
            var gottenMessage = await gottenHttpResponse.Content.ReadAsStringAsync();
            Assert.Contains("Requested data not found.", gottenMessage);
        }
    }
}