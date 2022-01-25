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

            HttpResponseMessage postedResponse = await httpClient.PostLoadTestConfig(LoadTestConfigsUri, this.output);
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
            HttpResponseMessage postResponse = await httpClient.PostLoadTestConfig(LoadTestConfigsUri, this.output);
            var postedLoadTestConfig = await postResponse.Content.ReadFromJsonAsync<LoadTestConfig>(this.jsonOptions);

            LoadTestConfig loadTestConfig = new ();

            loadTestConfig.SetMockData($"Updated CanPutLoadTestConfigs - IntegrationTesting-{nameof(this.CanPutLoadTestConfigs)}-{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}");

            var loadTestConfigPayload = loadTestConfig.AutomapAndGetLoadTestConfigTestPayload();

            // Update LoadTestConfig
            var puttedResponse = await httpClient.PutLoadTestConfigById(LoadTestConfigsUri, postedLoadTestConfig.Id, loadTestConfigPayload, this.output);

            Assert.Equal(HttpStatusCode.NoContent, puttedResponse.StatusCode);

            var gottenResponse = await httpClient.GetItemById<LoadTestConfig>(LoadTestConfigsUri, postedLoadTestConfig.Id, this.output);
            var gottenTestRun = await gottenResponse.Content.ReadFromJsonAsync<LoadTestConfig>(this.jsonOptions);
            postedLoadTestConfig.Name = loadTestConfigPayload.Name;

            Assert.Equal(JsonSerializer.Serialize(postedLoadTestConfig), JsonSerializer.Serialize(gottenTestRun));
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

            HttpResponseMessage httpResponse = await httpClient.PostLoadTestConfig(LoadTestConfigsUri, this.output);
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
    }
}
