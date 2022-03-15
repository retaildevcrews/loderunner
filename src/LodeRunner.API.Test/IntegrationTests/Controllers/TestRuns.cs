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
using LodeRunner.API.Test.IntegrationTests.Extensions;
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
        private const string InvalidTestRunId = "xxxx-0000";

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

            HttpResponseMessage httpResponse = await httpClient.GetTestRuns(SystemConstants.CategoryTestRunsPath, this.output);

            var responseContents = await httpResponse.Content.ReadAsStringAsync();
            AssertExtension.Contains(httpResponse.StatusCode, new List<HttpStatusCode> { HttpStatusCode.OK, HttpStatusCode.NoContent }, responseContents);

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

            HttpResponseMessage postedResponse = await httpClient.PostTestRun(SystemConstants.CategoryTestRunsPath, this.output);

            AssertExtension.EqualResponseStatusCode(HttpStatusCode.Created, postedResponse);

            var postedTestRun = await postedResponse.Content.ReadFromJsonAsync<TestRun>(this.jsonOptions);
            var gottenHttpResponse = await httpClient.GetTestRunById(SystemConstants.CategoryTestRunsPath, postedTestRun.Id, this.output);

            AssertExtension.EqualResponseStatusCode(HttpStatusCode.OK, gottenHttpResponse);

            var gottenTestRun = await gottenHttpResponse.Content.ReadFromJsonAsync<TestRun>(this.jsonOptions);

            Assert.Equal(JsonSerializer.Serialize(postedTestRun), JsonSerializer.Serialize(gottenTestRun));

            // Delete the TestRun created in this Integration Test scope
            await httpClient.DeleteTestRunById(SystemConstants.CategoryTestRunsPath, gottenTestRun.Id, this.output);
        }

        /// <summary>
        /// Determines whether this instance [returns correct response when it fails get a test run by invalid ID].
        /// </summary>
        /// <returns><see cref="Task"/> representing the asynchronous integration test.</returns>
        [Fact]
        [Trait("Category", "Integration")]
        public async Task CannotGetTestRunByInvalidId()
        {
            using var httpClient = ComponentsFactory.CreateLodeRunnerAPIHttpClient(this.factory);

            var returnedHttpResponse = await httpClient.GetTestRunById(SystemConstants.CategoryTestRunsPath, InvalidTestRunId, this.output);

            AssertExtension.EqualResponseStatusCode(HttpStatusCode.BadRequest, returnedHttpResponse);
        }

        /// <summary>
        /// Determines whether this instance [returns correct response when it fails to post an invalid test run].
        /// </summary>
        /// <returns><see cref="Task"/> representing the asynchronous integration test.</returns>
        [Fact]
        [Trait("Category", "Integration")]
        public async Task CannotPostInvalidTestRun()
        {
            using var httpClient = ComponentsFactory.CreateLodeRunnerAPIHttpClient(this.factory);

            // Create testRun payload; update StartTime and CompletedTime to make it invalid.
            TestRunPayload testRunPayload = new ();
            testRunPayload.SetMockData($"Sample TestRun - IntegrationTesting-{nameof(this.CannotPostInvalidTestRun)}-{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}");
            testRunPayload.StartTime = DateTime.UtcNow.AddMinutes(10);
            testRunPayload.CompletedTime = DateTime.UtcNow;

            var returnedHttpResponse = await httpClient.PostEntity<TestRun, TestRunPayload>(testRunPayload, SystemConstants.CategoryTestRunsPath, this.output);

            AssertExtension.EqualResponseStatusCode(HttpStatusCode.BadRequest, returnedHttpResponse);
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
            HttpResponseMessage postResponse = await httpClient.PostTestRun(SystemConstants.CategoryTestRunsPath, this.output);
            var postedTestRun = await postResponse.Content.ReadFromJsonAsync<TestRun>(this.jsonOptions);

            TestRunPayload testRunPayload = new ();
            testRunPayload.Name = $"Updated TestRun - IntegrationTesting-{nameof(this.CanPutTestRuns)}-{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}";
            testRunPayload.CreatedTime = postedTestRun.CreatedTime;
            testRunPayload.StartTime = postedTestRun.StartTime;
            testRunPayload.LoadTestConfig = postedTestRun.LoadTestConfig;
            testRunPayload.LoadClients = postedTestRun.LoadClients;

            // Update TestRun
            var puttedResponse = await httpClient.PutTestRunById(SystemConstants.CategoryTestRunsPath, postedTestRun.Id, testRunPayload, this.output);

            AssertExtension.EqualResponseStatusCode(HttpStatusCode.NoContent, puttedResponse);

            var gottenResponse = await httpClient.GetTestRunById(SystemConstants.CategoryTestRunsPath, postedTestRun.Id, this.output);
            var gottenTestRun = await gottenResponse.Content.ReadFromJsonAsync<TestRun>(this.jsonOptions);

            // We test for NotEqual since we have updated testRunPayload.Name as part of Put at the previous step few lines above.
            Assert.NotEqual(JsonSerializer.Serialize(postedTestRun), JsonSerializer.Serialize(gottenTestRun));

            // For validation purpose we are reusing the original "postedTestRun" object, now we set "postedTestRun.Name" to "testRunPayload.Name"
            postedTestRun.Name = testRunPayload.Name;

            // We test for Equal, now the Expected value "postedTestRun" should be equals that the Actual value gottenTestRun
            Assert.Equal(JsonSerializer.Serialize(postedTestRun), JsonSerializer.Serialize(gottenTestRun));
        }

        /// <summary>
        /// Determines whether this instance [can update test run with an invalid payload].
        /// </summary>
        /// <returns><see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("Category", "Integration")]
        public async Task CannotPutInvalidTestRun()
        {
            using var httpClient = ComponentsFactory.CreateLodeRunnerAPIHttpClient(this.factory);

            // Create a new TestRun
            HttpResponseMessage postResponse = await httpClient.PostTestRun(SystemConstants.CategoryTestRunsPath, this.output);
            var postedTestRun = await postResponse.Content.ReadFromJsonAsync<TestRun>(this.jsonOptions);

            TestRunPayload testRunPayload = new ();
            testRunPayload.Name = $"Updated TestRun - IntegrationTesting-{nameof(this.CannotPutInvalidTestRun)}-{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}";
            testRunPayload.CreatedTime = postedTestRun.CreatedTime;
            testRunPayload.StartTime = DateTime.UtcNow.AddMinutes(5);
            testRunPayload.LoadTestConfig = postedTestRun.LoadTestConfig;
            testRunPayload.LoadClients = postedTestRun.LoadClients;
            testRunPayload.CompletedTime = DateTime.UtcNow;

            // Update TestRun
            var puttedResponse = await httpClient.PutTestRunById(SystemConstants.CategoryTestRunsPath, postedTestRun.Id, testRunPayload, this.output);
            AssertExtension.EqualResponseStatusCode(HttpStatusCode.BadRequest, puttedResponse);
        }

        /// <summary>
        /// Determines whether this instance [can update a test run with an improperly formatted identifier].
        /// </summary>
        /// <returns><see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("Category", "Integration")]
        public async Task CannotPutTestRunWithInvalidId()
        {
            using var httpClient = ComponentsFactory.CreateLodeRunnerAPIHttpClient(this.factory);

            TestRunPayload testRunPayload = new ();
            testRunPayload.Name = $"Updated TestRun - IntegrationTesting-{nameof(this.CannotPutInvalidTestRun)}-{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}";
            testRunPayload.CreatedTime = DateTime.UtcNow;
            testRunPayload.StartTime = DateTime.UtcNow;

            // Update TestRun
            var puttedResponse = await httpClient.PutTestRunById(SystemConstants.CategoryTestRunsPath, InvalidTestRunId, testRunPayload, this.output);
            AssertExtension.EqualResponseStatusCode(HttpStatusCode.BadRequest, puttedResponse);
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
            var testRunSvc = ComponentsFactory.GetTestRunService(this.factory);
            HttpResponseMessage httpResponse = await httpClient.PostTestRun(SystemConstants.CategoryTestRunsPath, this.output);
            var testRun = await httpResponse.Content.ReadFromJsonAsync<TestRun>(this.jsonOptions);

            // Now try to delete, and it should return 409 Conflict,
            // Since the CompletedTime property doesn't exist
            // Meaning the Test is still running
            // Try Deleting the TestRun created in this Integration Test scope
            var response = await httpClient.DeleteTestRunById(SystemConstants.CategoryTestRunsPath, testRun.Id, this.output);
            Assert.Null(testRun.CompletedTime);

            AssertExtension.EqualResponseStatusCode(HttpStatusCode.Conflict, response);

            // TODO: When TestRun feature is completed, refactor to add deletion test
            // Save the completed time and update CosmosDB
            testRun.CompletedTime = DateTime.UtcNow;
            _ = await testRunSvc.Post(testRun, System.Threading.CancellationToken.None);

            // Try deleting the TestRun created, which should return NoContent
            response = await httpClient.DeleteTestRunById(SystemConstants.CategoryTestRunsPath, testRun.Id, this.output);

            AssertExtension.EqualResponseStatusCode(HttpStatusCode.NoContent, response);
            Assert.Equal(0, response.Content.Headers.ContentLength);

            // Trying to delete the old TestRun should result in NotFound
            response = await httpClient.DeleteTestRunById(SystemConstants.CategoryTestRunsPath, testRun.Id, this.output);

            AssertExtension.EqualResponseStatusCode(HttpStatusCode.NotFound, response);

            var gottenHttpResponse = await httpClient.GetTestRunById(SystemConstants.CategoryTestRunsPath, testRun.Id, this.output);

            AssertExtension.EqualResponseStatusCode(HttpStatusCode.NotFound, gottenHttpResponse);

            var gottenMessage = await gottenHttpResponse.Content.ReadAsStringAsync();
            Assert.Contains("Not Found", gottenMessage);

            // Ensure that PUT still works as expected (fails to update deleted item).
            TestRunPayload testRunPayload = new ();
            testRunPayload.Name = $"Updated TestRun - IntegrationTesting-{nameof(this.CanDeleteTestRunById)}-{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}";
            testRunPayload.CreatedTime = DateTime.UtcNow;
            testRunPayload.StartTime = DateTime.UtcNow.AddMinutes(10);
            testRunPayload.LoadTestConfig = testRun.LoadTestConfig;
            testRunPayload.LoadClients = testRun.LoadClients;

            var puttedResponse = await httpClient.PutTestRunById(SystemConstants.CategoryTestRunsPath, testRun.Id, testRunPayload, this.output);
            AssertExtension.EqualResponseStatusCode(HttpStatusCode.NotFound, puttedResponse);
        }
    }
}
