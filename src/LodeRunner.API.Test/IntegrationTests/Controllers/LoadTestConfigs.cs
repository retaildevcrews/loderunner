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
using LodeRunner.API.Test.IntegrationTests.Extensions;
using LodeRunner.Core.Automapper;
using LodeRunner.Core.Models;
using Xunit;
using Xunit.Abstractions;

namespace LodeRunner.API.Test.IntegrationTests.Controllers
{
    /// <summary>
    /// Represents LoadTestConfigs.
    /// </summary>
    public class LoadTestConfigs : IClassFixture<ApiWebApplicationFactory<Startup>>
    {
        private const string InvalidLoadTestConfigId = "xxxx-0000";

        private readonly ApiWebApplicationFactory<Startup> factory;

        private readonly JsonSerializerOptions jsonOptions;

        private readonly ITestOutputHelper output;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadTestConfigs"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="output">The output.</param>
        public LoadTestConfigs(ApiWebApplicationFactory<Startup> factory, ITestOutputHelper output)
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
        /// Determines whether this instance [can get Load Test Configs].
        /// </summary>
        /// <returns><see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("Category", "Integration")]
        public async Task CanGetLoadTestConfigs()
        {
            using var httpClient = ComponentsFactory.CreateLodeRunnerAPIHttpClient(this.factory);

            HttpResponseMessage httpResponse = await httpClient.GetAllItems<LoadTestConfig>(SystemConstants.CategoryLoadTestConfigsPath, this.output);

            var responseContents = await httpResponse.Content.ReadAsStringAsync();
            AssertExtension.Contains(httpResponse.StatusCode, new List<HttpStatusCode> { HttpStatusCode.OK, HttpStatusCode.NoContent }, responseContents);

            if (httpResponse.StatusCode == HttpStatusCode.OK)
            {
                // TODO: Separate out to test GetAllLoadTestConfig with OK response
                var testRuns = await httpResponse.Content.ReadFromJsonAsync<List<LoadTestConfig>>(this.jsonOptions);
                Assert.NotEmpty(testRuns);
            }
            else
            {
                // TODO: Separate out to test GetAllLoadTestConfig with No Content response
                Assert.Equal(0, httpResponse.Content.Headers.ContentLength);
            }
        }

        /// <summary>
        /// Determines whether this instance [can post a load test config and get a load test config by ID].
        /// </summary>
        /// <returns><see cref="Task"/> representing the asynchronous integration test.</returns>
        [Fact]
        [Trait("Category", "Integration")]
        public async Task CanCreateAndGetLoadTestConfigById()
        {
            using var httpClient = ComponentsFactory.CreateLodeRunnerAPIHttpClient(this.factory);

            var loadTestConfigPayload = this.GetLoadTestConfigPayloadWithDefaultMockData("Sample -  LoadTestConfig");

            HttpResponseMessage postedResponse = await httpClient.PostEntity<LoadTestConfig, LoadTestConfigPayload>(loadTestConfigPayload, SystemConstants.CategoryLoadTestConfigsPath, this.output);

            AssertExtension.EqualResponseStatusCode(HttpStatusCode.Created, postedResponse);

            var postedTestRun = await postedResponse.Content.ReadFromJsonAsync<LoadTestConfig>(this.jsonOptions);
            var gottenHttpResponse = await httpClient.GetItemById<LoadTestConfig>(SystemConstants.CategoryLoadTestConfigsPath, postedTestRun.Id, this.output);

            AssertExtension.EqualResponseStatusCode(HttpStatusCode.OK, gottenHttpResponse);

            var gottenTestRun = await gottenHttpResponse.Content.ReadFromJsonAsync<LoadTestConfig>(this.jsonOptions);

            Assert.Equal(JsonSerializer.Serialize(postedTestRun), JsonSerializer.Serialize(gottenTestRun));

            // Delete the LoadTestConfig created in this Integration Test scope
            await httpClient.DeleteItemById<LoadTestConfig>(SystemConstants.CategoryLoadTestConfigsPath, gottenTestRun.Id, this.output);
        }

        /// <summary>
        /// Determines whether this instance [returns correct response when it fails get a test run by invalid ID].
        /// </summary>
        /// <returns><see cref="Task"/> representing the asynchronous integration test.</returns>
        [Fact]
        [Trait("Category", "Integration")]
        public async Task CannotGetLoadTestConfigByInvalidId()
        {
            using var httpClient = ComponentsFactory.CreateLodeRunnerAPIHttpClient(this.factory);

            var returnedHttpResponse = await httpClient.GetItemById<LoadTestConfig>(SystemConstants.CategoryTestRunsPath, InvalidLoadTestConfigId, this.output);

            AssertExtension.EqualResponseStatusCode(HttpStatusCode.BadRequest, returnedHttpResponse);
        }

        /// <summary>
        /// Determines whether this instance [returns correct response when it fails to post an invalid load test config].
        /// </summary>
        /// <returns><see cref="Task"/> representing the asynchronous integration test.</returns>
        [Fact]
        [Trait("Category", "Integration")]
        public async Task CannotPostInvalidLoadTestConfig()
        {
            using var httpClient = ComponentsFactory.CreateLodeRunnerAPIHttpClient(this.factory);

            // Create testRun payload; update "Files" property to make the payload invalid.
            var loadTestConfigPayload = this.GetLoadTestConfigPayloadWithDefaultMockData("Sample - LoadTestConfig");
            loadTestConfigPayload.Files = null;

            var returnedHttpResponse = await httpClient.PostEntity<LoadTestConfig, LoadTestConfigPayload>(loadTestConfigPayload, SystemConstants.CategoryLoadTestConfigsPath, this.output);

            AssertExtension.EqualResponseStatusCode(HttpStatusCode.BadRequest, returnedHttpResponse);

            loadTestConfigPayload.Files = new List<string>() { "baseline.json", "benchmark.json" };
            loadTestConfigPayload.Server = null;

            returnedHttpResponse = await httpClient.PostEntity<LoadTestConfig, LoadTestConfigPayload>(loadTestConfigPayload, SystemConstants.CategoryLoadTestConfigsPath, this.output);

            AssertExtension.EqualResponseStatusCode(HttpStatusCode.BadRequest, returnedHttpResponse);
        }

        /// <summary>
        /// Determines whether this instance [can update loadTestConfig by identifier].
        /// </summary>
        /// <returns><see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("Category", "Integration")]
        public async Task CanPutLoadTestConfigs()
        {
            using var httpClient = ComponentsFactory.CreateLodeRunnerAPIHttpClient(this.factory);

            // Create a new LoadTestConfig
            var loadTestConfigPayload = this.GetLoadTestConfigPayloadWithDefaultMockData("Sample - LoadTestConfig");

            HttpResponseMessage postedResponse = await httpClient.PostEntity<LoadTestConfig, LoadTestConfigPayload>(loadTestConfigPayload, SystemConstants.CategoryLoadTestConfigsPath, this.output);

            var postedLoadTestConfig = await postedResponse.Content.ReadFromJsonAsync<LoadTestConfig>(this.jsonOptions);

            var updatedloadTestConfigPayload = this.GetLoadTestConfigPayloadWithDefaultMockData("Updated - LoadTestConfigs");

            // Update LoadTestConfig
            var puttedResponse = await httpClient.PutEntityByItemId<LoadTestConfig, LoadTestConfigPayload>(SystemConstants.CategoryLoadTestConfigsPath, postedLoadTestConfig.Id, updatedloadTestConfigPayload, this.output);

            AssertExtension.EqualResponseStatusCode(HttpStatusCode.NoContent, puttedResponse);

            var gottenResponse = await httpClient.GetItemById<LoadTestConfig>(SystemConstants.CategoryLoadTestConfigsPath, postedLoadTestConfig.Id, this.output);
            var actualLoadTestConfig = await gottenResponse.Content.ReadFromJsonAsync<LoadTestConfig>(this.jsonOptions);

            // We create a expected object to validate.
            var expectedLoadTestConfig = postedLoadTestConfig.Clone();

            // We test for NotEqual since we have updated loadTestConfigPayload.Name as part of Put at the previous step few lines above.
            Assert.NotEqual(JsonSerializer.Serialize(expectedLoadTestConfig), JsonSerializer.Serialize(actualLoadTestConfig));

            // For validation purpose we are reusing the original "postedTestRun" object, that we already mapped to "expectedLoadTestConfig",
            // now we set "expectedLoadTestConfig.Name" to  "updatedloadTestConfigPayload.Name"
            expectedLoadTestConfig.Name = updatedloadTestConfigPayload.Name;

            // We test for Equal after have updated original postedTestRun.Name to match the loadTestConfigPayload.Name
            Assert.Equal(JsonSerializer.Serialize(expectedLoadTestConfig), JsonSerializer.Serialize(actualLoadTestConfig));
        }

        /// <summary>
        /// Determines whether this instance [can update loadTestConfig with an improperly formatted identifier].
        /// </summary>
        /// <returns><see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("Category", "Integration")]
        public async Task CannotPutLoadTestConfigsWithInvalidId()
        {
            using var httpClient = ComponentsFactory.CreateLodeRunnerAPIHttpClient(this.factory);

            var updatedloadTestConfigPayload = this.GetLoadTestConfigPayloadWithDefaultMockData("Updated - LoadTestConfigs");

            // Update LoadTestConfig
            var puttedResponse = await httpClient.PutEntityByItemId<LoadTestConfig, LoadTestConfigPayload>(SystemConstants.CategoryLoadTestConfigsPath, InvalidLoadTestConfigId, updatedloadTestConfigPayload, this.output);

            AssertExtension.EqualResponseStatusCode(HttpStatusCode.BadRequest, puttedResponse);
        }

        /// <summary>
        /// Determines whether this instance [can update load test config with an invalid payload].
        /// </summary>
        /// <returns><see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("Category", "Integration")]
        public async Task CannotPutInvalidLoadTestConfig()
        {
            using var httpClient = ComponentsFactory.CreateLodeRunnerAPIHttpClient(this.factory);

            // Create loadTestConfig payload; update "Files" property to make the payload invalid.
            var loadTestConfigPayload = this.GetLoadTestConfigPayloadWithDefaultMockData("Sample - LoadTestConfig");

            var returnedHttpResponse = await httpClient.PostEntity<LoadTestConfig, LoadTestConfigPayload>(loadTestConfigPayload, SystemConstants.CategoryLoadTestConfigsPath, this.output);

            AssertExtension.EqualResponseStatusCode(HttpStatusCode.Created, returnedHttpResponse);

            var postedLoadTestConfig = await returnedHttpResponse.Content.ReadFromJsonAsync<LoadTestConfig>(this.jsonOptions);

            // Update LoadTestConfig
            loadTestConfigPayload.Files = null;
            HttpResponseMessage puttedResponse = await httpClient.PutEntityByItemId<LoadTestConfig, LoadTestConfigPayload>(SystemConstants.CategoryTestRunsPath, postedLoadTestConfig.Id, loadTestConfigPayload, this.output);

            AssertExtension.EqualResponseStatusCode(HttpStatusCode.BadRequest, puttedResponse);

            loadTestConfigPayload.Files = new List<string>() { "baseline.json", "benchmark.json" };
            loadTestConfigPayload.Server = null;

            returnedHttpResponse = await httpClient.PutEntityByItemId<LoadTestConfig, LoadTestConfigPayload>(SystemConstants.CategoryTestRunsPath, postedLoadTestConfig.Id, loadTestConfigPayload, this.output);

            AssertExtension.EqualResponseStatusCode(HttpStatusCode.BadRequest, returnedHttpResponse);
        }

        /// <summary>
        /// Determines whether this instance [can delete a loadTestConfig by ID].
        /// </summary>
        /// <returns><see cref="Task"/> representing the asynchronous integration test.</returns>
        [Fact]
        [Trait("Category", "Integration")]
        public async Task CanDeleteLoadTestConfigById()
        {
            using var httpClient = ComponentsFactory.CreateLodeRunnerAPIHttpClient(this.factory);

            var loadTestConfigPayload = this.GetLoadTestConfigPayloadWithDefaultMockData("Sample - LoadTestConfig");

            HttpResponseMessage httpResponse = await httpClient.PostEntity<LoadTestConfig, LoadTestConfigPayload>(loadTestConfigPayload, SystemConstants.CategoryLoadTestConfigsPath, this.output);

            var loadTestConfig = await httpResponse.Content.ReadFromJsonAsync<LoadTestConfig>(this.jsonOptions);

            // Delete the LoadTestConfig created in this Integration Test scope
            var deletedResponse = await httpClient.DeleteItemById<LoadTestConfig>(SystemConstants.CategoryLoadTestConfigsPath, loadTestConfig.Id, this.output);

            AssertExtension.EqualResponseStatusCode(HttpStatusCode.NoContent, deletedResponse);

            Assert.Equal(0, deletedResponse.Content.Headers.ContentLength);

            var gottenHttpResponse = await httpClient.GetItemById<LoadTestConfig>(SystemConstants.CategoryLoadTestConfigsPath, loadTestConfig.Id, this.output);
            AssertExtension.EqualResponseStatusCode(HttpStatusCode.NotFound, gottenHttpResponse);

            var gottenMessage = await gottenHttpResponse.Content.ReadAsStringAsync();
            Assert.Contains("Not Found", gottenMessage);

            // Ensure that PUT works as expected on deleted item
            var puttedResponse = await httpClient.PutEntityByItemId<LoadTestConfig, LoadTestConfigPayload>(SystemConstants.CategoryLoadTestConfigsPath, loadTestConfig.Id, loadTestConfigPayload, this.output);

            AssertExtension.EqualResponseStatusCode(HttpStatusCode.NotFound, puttedResponse);
        }

        /// <summary>
        /// Gets the load test configuration payload with mock data.
        /// </summary>
        /// <param name="configNamePrefix">Name to be set to entity's name property.</param>
        /// <param name="methodName">Caller Name.</param>
        /// <returns>LoadTestConfigPayload.</returns>
        private LoadTestConfigPayload GetLoadTestConfigPayloadWithDefaultMockData(string configNamePrefix, [CallerMemberName] string methodName = nameof(this.GetLoadTestConfigPayloadWithDefaultMockData))
        {
            string entityName = $"{configNamePrefix} - IntegrationTesting-{methodName}-{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}";
            LoadTestConfig loadTestConfig = new ();
            loadTestConfig.SetMockData(entityName);
            return loadTestConfig.MapLoadTestConfigToPayload();
        }
    }
}
