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
using LodeRunner.API.Test.IntegrationTests.AutoMapper;
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
        private const string LoadTestConfigsUri = "/api/LoadTestConfigs";

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

            HttpResponseMessage httpResponse = await httpClient.GetAllItems<LoadTestConfig>(LoadTestConfigsUri, this.output);
            Assert.Contains(httpResponse.StatusCode, new List<HttpStatusCode> { HttpStatusCode.OK, HttpStatusCode.NoContent });

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

            HttpResponseMessage postedResponse = await httpClient.PostEntity<LoadTestConfig, LoadTestConfigPayload>(loadTestConfigPayload, LoadTestConfigsUri, this.output);

            Assert.Equal(HttpStatusCode.Created, postedResponse.StatusCode);

            var postedTestRun = await postedResponse.Content.ReadFromJsonAsync<LoadTestConfig>(this.jsonOptions);
            var gottenHttpResponse = await httpClient.GetItemById<LoadTestConfig>(LoadTestConfigsUri, postedTestRun.Id, this.output);

            Assert.Equal(HttpStatusCode.OK, gottenHttpResponse.StatusCode);
            var gottenTestRun = await gottenHttpResponse.Content.ReadFromJsonAsync<LoadTestConfig>(this.jsonOptions);

            Assert.Equal(JsonSerializer.Serialize(postedTestRun), JsonSerializer.Serialize(gottenTestRun));

            // Delete the LoadTestConfig created in this Integration Test scope
            await httpClient.DeleteItemById<LoadTestConfig>(LoadTestConfigsUri, gottenTestRun.Id, this.output);
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

            HttpResponseMessage postedResponse = await httpClient.PostEntity<LoadTestConfig, LoadTestConfigPayload>(loadTestConfigPayload, LoadTestConfigsUri, this.output);

            var postedLoadTestConfig = await postedResponse.Content.ReadFromJsonAsync<LoadTestConfig>(this.jsonOptions);

            var updatedloadTestConfigPayload = this.GetLoadTestConfigPayloadWithDefaultMockData("Updated - LoadTestConfigs");

            // Update LoadTestConfig
            var puttedResponse = await httpClient.PutEntityByItemId<LoadTestConfig, LoadTestConfigPayload>(LoadTestConfigsUri, postedLoadTestConfig.Id, updatedloadTestConfigPayload, this.output);

            Assert.Equal(HttpStatusCode.NoContent, puttedResponse.StatusCode);

            var gottenResponse = await httpClient.GetItemById<LoadTestConfig>(LoadTestConfigsUri, postedLoadTestConfig.Id, this.output);
            var actualLoadTestConfig = await gottenResponse.Content.ReadFromJsonAsync<LoadTestConfig>(this.jsonOptions);

            // We create a expected object to validate.
            var expectedLoadTestConfig = postedLoadTestConfig.AutomapEntity();

            // We test for NotEqual since we have updated loadTestConfigPayload.Name as part of Put at the previous step few lines above.
            Assert.NotEqual(JsonSerializer.Serialize(expectedLoadTestConfig), JsonSerializer.Serialize(actualLoadTestConfig));

            // For validation purpose we are reusing the original "postedTestRun" object, that we already mapped to "expectedLoadTestConfig",
            // now we set "expectedLoadTestConfig.Name" to  "updatedloadTestConfigPayload.Name"
            expectedLoadTestConfig.Name = updatedloadTestConfigPayload.Name;

            // We test for Equal after have updated original postedTestRun.Name to match the loadTestConfigPayload.Name
            Assert.Equal(JsonSerializer.Serialize(expectedLoadTestConfig), JsonSerializer.Serialize(actualLoadTestConfig));
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

            HttpResponseMessage httpResponse = await httpClient.PostEntity<LoadTestConfig, LoadTestConfigPayload>(loadTestConfigPayload, LoadTestConfigsUri, this.output);

            var loadTestConfig = await httpResponse.Content.ReadFromJsonAsync<LoadTestConfig>(this.jsonOptions);

            // Delete the LoadTestConfig created in this Integration Test scope
            var deletedResponse = await httpClient.DeleteItemById<LoadTestConfig>(LoadTestConfigsUri, loadTestConfig.Id, this.output);
            Assert.Equal(HttpStatusCode.NoContent, deletedResponse.StatusCode);

            Assert.Equal(0, deletedResponse.Content.Headers.ContentLength);

            var gottenHttpResponse = await httpClient.GetItemById<LoadTestConfig>(LoadTestConfigsUri, loadTestConfig.Id, this.output);
            Assert.Equal(HttpStatusCode.NotFound, gottenHttpResponse.StatusCode);
            var gottenMessage = await gottenHttpResponse.Content.ReadAsStringAsync();
            Assert.Contains("Requested data not found.", gottenMessage);
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
            return loadTestConfig.AutomapAndGetLoadTestConfigTestPayload();
        }
    }
}
