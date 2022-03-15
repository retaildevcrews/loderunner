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
using LodeRunner.API.Test.IntegrationTests.Extensions;
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

            var responseContents = await httpResponse.Content.ReadAsStringAsync();
            AssertExtension.Equal(expectedValue, httpResponse.Content.Headers.ContentType.ToString(), responseContents);
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

            HttpResponseMessage httpResponse = await httpClient.GetRetryAsync(SystemConstants.CategoryClientsPath, action, this.output, 5);

            if (httpResponse.StatusCode == HttpStatusCode.OK || httpResponse.StatusCode == HttpStatusCode.NoContent)
            {
                this.output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: {action}\tResponse StatusCode: '{httpResponse.StatusCode}'");
            }
            else
            {
                this.output.WriteLine($"UTC Time:{DateTime.UtcNow}\tAction: {action}\tUNEXPECTED Response StatusCode: '{httpResponse.StatusCode}'");
            }

            AssertExtension.EqualResponseStatusCode(HttpStatusCode.OK, httpResponse);

            List<Client> clients = await httpResponse.Content.ReadFromJsonAsync<List<Client>>(this.jsonOptions);

            Assert.NotEmpty(clients);

            var minDate = new DateTime(1900, 1, 1);

            // Assert for all required fields
            clients.ForEach((c) =>
            {
                Assert.NotNull(c.Version);
                Assert.NotNull(c.Region);
                Assert.NotNull(c.StartupArgs);
                Assert.NotNull(c.ClientStatusId);
                Assert.NotNull(c.LoadClientId);
                Assert.True(c.StartTime >= minDate);
                Assert.True(c.LastStatusChange >= minDate);
                Assert.True(c.LastUpdated >= minDate);
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

            (HttpResponseMessage httpResponseReady, Client readyClient) = await httpClient.GetClientByIdRetriesAsync(SystemConstants.CategoryClientsPath, clientStatusId, ClientStatusType.Ready, this.jsonOptions, this.output);

            AssertExtension.EqualResponseStatusCode(HttpStatusCode.OK, httpResponseReady);

            Assert.NotNull(readyClient);
            Assert.Equal(clientStatusId, readyClient.ClientStatusId);

            l8rService.StopService();
            this.output.WriteLine($"Stopping LodeRunner (client mode) [ClientStatusId: {clientStatusId}]");

            (HttpResponseMessage httpResponseTerminating, Client terminatingClient) = await httpClient.GetClientByIdRetriesAsync(SystemConstants.CategoryClientsPath, clientStatusId, ClientStatusType.Terminating, this.jsonOptions, this.output);

            AssertExtension.EqualResponseStatusCode(HttpStatusCode.OK, httpResponseTerminating);

            Assert.NotNull(terminatingClient);
            Assert.Equal(clientStatusId, terminatingClient.ClientStatusId);
        }
    }
}
