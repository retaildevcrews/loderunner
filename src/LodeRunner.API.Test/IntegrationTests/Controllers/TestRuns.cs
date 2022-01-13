﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
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
        private const string GetOrPostTestRunsUri = "/api/TestRuns";
        private const string DeleteOrPutTestRunsByIdUri = "/api/TestRuns/{testRunId}";
        private const string PostClientResultsByTestRunIdUri = "/api/TestRuns/{testRunId}/ClientResults";

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

            // TODO need to create one to make sure we can get at least on back.

            await httpClient.GetTestRuns(GetOrPostTestRunsUri, this.jsonOptions, this.output);
        }



        /// <summary>
        /// Determines whether this instance [can put test runs].
        /// </summary>
        /// <returns><see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("Category", "Integration")]
        private async Task CanPutTestRuns()
        {
            using var httpClient = ComponentsFactory.CreateLodeRunnerAPIHttpClient(this.factory);

            // TODO need to create one to make sure we can get at least on back.

            await httpClient.PutTestRun(GetOrPostTestRunsUri, this.jsonOptions, this.output);
        }
    }
}
