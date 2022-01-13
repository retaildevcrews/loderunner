// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using LodeRunner.API.Middleware;
using LodeRunner.API.Test.IntegrationTests.Payloads;
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
        /// Gets endpoints return success and correct content.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="expectedValue">The expectedValue.</param>
        /// <returns><see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [Trait("Category", "Integration")]
        [InlineData("/", "text/html; charset=utf-8")]
        [InlineData("/version", "text/plain")]
        public async Task CanGetEndpointsReturnSuccessAndCorrectContentType(string url, string expectedValue)
        {
            using var httpClient = ComponentsFactory.CreateLodeRunnerAPIHttpClient(this.factory);

            var httpResponse = await httpClient.GetAsync(url);

            if (httpResponse.IsSuccessStatusCode)
            {
                Assert.Equal(expectedValue, httpResponse.Content.Headers.ContentType.ToString());
            }
            else
            {
                Assert.True(false, $"Unable to process {nameof(this.CanGetEndpointsReturnSuccessAndCorrectContentType)} request.");
            }
        }

        /// <summary>
        /// Determines whether this instance [can get test runs].
        /// </summary>
        /// <returns><see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("Category", "Integration")]
        private async Task CanGetTestRuns()
        {
            using var httpClient = ComponentsFactory.CreateLodeRunnerAPIHttpClient(this.factory);

            await httpClient.GetTestRuns(TestRunsUri, this.jsonOptions, this.output);
        }

        /// <summary>
        /// Determines whether this instance [can put test runs].
        /// </summary>
        private async Task CanCreateAndGetTestRun()
        {
            using var httpClient = ComponentsFactory.CreateLodeRunnerAPIHttpClient(this.factory);

            string json = JsonSerializer.Serialize(new TestRunTestPayload(), this.jsonOptions);
            var payloadContent = new StringContent(json, Encoding.UTF8, "application/json");
            var httpPostResponse = await httpClient.PostAsync(TestRunsUri, payloadContent);

            Assert.Equal(System.Net.HttpStatusCode.Created, httpPostResponse.StatusCode);

            string postResponseContent = await httpPostResponse.Content.ReadAsStringAsync();
            TestRun postedTestRun = JsonSerializer.Deserialize<TestRun>(postResponseContent);
            var httpGetResponse = await httpClient.GetAsync(TestRunsUri + "/" + postedTestRun.Id);
            string getResponseContent = await httpGetResponse.Content.ReadAsStringAsync();
            TestRun gettedTestRun = JsonSerializer.Deserialize<TestRun>(getResponseContent);

            Assert.Equal(postedTestRun, gettedTestRun);
        }

        /// <summary>
        /// Determines whether this instance [can get clients by identifier] the specified client status identifier.
        /// </summary>
        /// <returns><see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("Category", "Integration")]
        private async Task CanPutTestRuns()
        {
            using var httpClient = ComponentsFactory.CreateLodeRunnerAPIHttpClient(this.factory);

            var testRun = await httpClient.PostTestRun(TestRunsUri, this.jsonOptions, this.output);

            Assert.NotNull(testRun);

            var errorsCount = ParametersValidator<TestRun>.ValidateEntityId(testRun.Id).Count;

            Assert.True(errorsCount == 0, $"Local Time:{DateTime.Now}\tUnable to Post a Sample TestRun item.");

            await httpClient.PutTestRun(testRun, $"{TestRunsUri}/{testRun.Id}", this.jsonOptions, this.output);
        }
    }
}
