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
using LodeRunner.API.Models;
using LodeRunner.Core.Models;
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
        public async Task CanGetEndpointsReturnSuccessAndCorrectContentType(string url, string expectedValue)
        {
            // TODO: This does not test clients endpoint. Please move to different file.
            using var httpClient = ComponentsFactory.CreateLodeRunnerAPIHttpClient(this.factory);

            var httpResponse = await httpClient.GetAsync(url);

            if (httpResponse.StatusCode == HttpStatusCode.OK)
            {
                this.output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: GET Url '{url}'\tResponse StatusCode: 'OK'");
            }
            else
            {
                this.output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: GET Url '{url}'\tUNEXPECTED Response StatusCode: '{httpResponse.StatusCode}'");
            }

            // TODO: Refactor to single line assert
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
        /// Determines whether this instance [can get clients].
        /// </summary>
        /// <returns><see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("Category", "Integration")]
        public async Task CanGetClients()
        {
            string action = "GET all Clients";
            using var httpClient = ComponentsFactory.CreateLodeRunnerAPIHttpClient(this.factory);
            using var l8rService = await ComponentsFactory.CreateAndStartLodeRunnerServiceInstance(nameof(this.CanGetClients));
            string clientStatusId = l8rService.ClientStatusId;

            this.output.WriteLine($"Started LodeRunner (client mode) [ClientStatusId: {clientStatusId}]");

            Assert.False(string.IsNullOrEmpty(clientStatusId), "Unable to retrieve ClientStatusId from LodeRunner (client mode) service.");

            HttpResponseMessage httpResponse = await httpClient.GetRetryAsync(ClientsUri, action, this.output, 5);

            if (httpResponse.StatusCode == HttpStatusCode.OK || httpResponse.StatusCode == HttpStatusCode.NoContent)
            {
                this.output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: {action}\tResponse StatusCode: '{httpResponse.StatusCode}'");
            }
            else
            {
                this.output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: {action}\tUNEXPECTED Response StatusCode: '{httpResponse.StatusCode}'");
            }

            Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);

            List<Client> clients = await httpResponse.Content.ReadFromJsonAsync<List<Client>>(this.jsonOptions);

            Assert.NotEmpty(clients);

            // Assert for all required fields
            clients.ForEach((c) => {
                Assert.NotNull(c.Version);
                Assert.NotNull(c.Region);
                Assert.NotNull(c.StartupArgs);
                Assert.NotNull(c.ClientStatusId);
                Assert.NotNull(c.LoadClientId);
                Assert.True(c.StartTime.CompareTo(new DateTime(1990, 1, 1, 00, 00, 00)) > 0);
                Assert.True(c.LastStatusChange.CompareTo(new DateTime(1990, 1, 1, 00, 00, 00)) > 0);
                Assert.True(c.LastUpdated.CompareTo(new DateTime(1990, 1, 1, 00, 00, 00)) > 0);
            });

            // TODO: Test for not found clients
        }

        /// <summary>
        /// Determines whether this instance [can get clients by identifier] the specified client status identifier.
        /// </summary>
        /// <returns><see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("Category", "Integration")]
        public async Task CanGetClientsById()
        {
            using var httpClient = ComponentsFactory.CreateLodeRunnerAPIHttpClient(this.factory);

            using var l8rService = await ComponentsFactory.CreateAndStartLodeRunnerServiceInstance(nameof(this.CanGetClientsById));
            string clientStatusId = l8rService.ClientStatusId;
            this.output.WriteLine($"Started LodeRunner (client mode) [ClientStatusId: {clientStatusId}]");

            Assert.False(string.IsNullOrEmpty(clientStatusId), "Unable to retrieve ClientStatusId from LodeRunner (client mode) service.");

            (HttpStatusCode readyStatusCode, Client readyClient) = await httpClient.GetClientByIdRetriesAsync(ClientsUri, clientStatusId, ClientStatusType.Ready, this.jsonOptions, this.output);

            Assert.Equal(HttpStatusCode.OK, readyStatusCode);
            Assert.NotNull(readyClient);
            Assert.Equal(clientStatusId, readyClient.ClientStatusId);

            l8rService.StopService();
            this.output.WriteLine($"Stopping LodeRunner (client mode) [ClientStatusId: {clientStatusId}]");

            (HttpStatusCode terminatingStatusCode, Client terminatingClient) = await httpClient.GetClientByIdRetriesAsync(ClientsUri, clientStatusId, ClientStatusType.Terminating, this.jsonOptions, this.output);

            Assert.Equal(HttpStatusCode.OK, terminatingStatusCode);
            Assert.NotNull(terminatingClient);
            Assert.Equal(clientStatusId, terminatingClient.ClientStatusId);
        }
    }
}
