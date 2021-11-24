// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.API.Models;
using LodeRunner.API.Services;
using LodeRunner.Core;
using LodeRunner.Core.CommandLine;
using LodeRunner.Core.Extensions;
using LodeRunner.Core.Models;
using LodeRunner.Services;
using Xunit;
using Xunit.Abstractions;

namespace LodeRunner.API.Test.IntegrationTests.Controllers
{
    /// <summary>
    /// Represents Clients.
    /// </summary>
    public class Clients : IClassFixture<ApiWebApplicationFactory<Startup>>
    {
        private const string ClientsUri = "/api/Clients";
        private const string ClientsByIdUri = "/api/Clients/";

        private readonly ApiWebApplicationFactory<Startup> factory;

        private readonly JsonSerializerOptions jsonOptions;

        private readonly ITestOutputHelper output;

        /// <summary>
        /// Initializes a new instance of the <see cref="Clients"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="output">The output.</param>
        public Clients(ApiWebApplicationFactory<Startup> factory, ITestOutputHelper output)
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
        public async Task GetEndpointsReturnSuccessAndCorrectContentType(string url, string expectedValue)
        {
            var httpClient = this.factory.CreateClient();

            // The endpoint or route of the controller action.
            var response = await httpClient.GetAsync(url);

            // Must be successful.
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal(expectedValue, response.Content.Headers.ContentType.ToString());
        }

        /// <summary>
        /// Determines whether this instance [can get clients].
        /// </summary>
        /// <returns><see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("Category", "Integration")]
        public async Task CanGetClients()
        {
            using var l8rService = await ComponentsFactory.CreateAndStartLodeRunnerServiceInstance(nameof(this.CanGetClients));

            string clientStatusId = l8rService.ClientStatusId;

            Assert.False(string.IsNullOrEmpty(clientStatusId), "Unable to retrieve ClientStatusId.");

            using var httpClient = ComponentsFactory.CreateLodeRunnerAPIHttpClient(this.factory);

            await httpClient.WaitAndValidateGetClientsForId(ClientsUri, clientStatusId, this.jsonOptions, this.output);
        }

        /// <summary>
        /// Determines whether this instance [can get clients by identifier] the specified client status identifier.
        /// </summary>
        /// <returns><see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("Category", "Integration")]
        public async Task CanGetClientsById()
        {
            using var l8rService = await ComponentsFactory.CreateAndStartLodeRunnerServiceInstance(nameof(this.CanGetClientsById));

            string clientStatusId = l8rService.ClientStatusId;

            Assert.False(string.IsNullOrEmpty(clientStatusId), "Unable to retrieve ClientStatusId.");

            using var httpClient = ComponentsFactory.CreateLodeRunnerAPIHttpClient(this.factory);

            await httpClient.WaitAndValidateGetByIdForStatus(ClientsByIdUri, clientStatusId, ClientStatusType.Ready, this.jsonOptions, this.output);

            l8rService.StopService();

            await httpClient.WaitAndValidateGetByIdForStatus(ClientsByIdUri, clientStatusId, ClientStatusType.Terminating, this.jsonOptions, this.output, 45000);
        }
    }
}
