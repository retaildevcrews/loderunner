// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LodeRunner.API.Models;
using LodeRunner.Core.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="Clients"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        public Clients(ApiWebApplicationFactory<Startup> factory)
        {
            this.factory = factory;
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
            var httpClient = this.factory.CreateClient();

            // The endpoint or route of the controller action.
            var httpResponse = await httpClient.GetAsync(ClientsUri);

            // Must be successful.
            httpResponse.EnsureSuccessStatusCode();

            // Deserialize and examine results.
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();

            var clients = JsonConvert.DeserializeObject<IEnumerable<ClientStatus>>(stringResponse);

            Assert.True(clients.Any(), "Cannot get any Clients");
        }

        /// <summary>
        /// Determines whether this instance [can get clients by identifier] the specified client status identifier.
        /// </summary>
        /// <param name="clientStatusId">The client status identifier.</param>
        /// <returns><see cref="Task"/> representing the asynchronous unit test.</returns>
        [Theory]
        [Trait("Category", "Integration")]
        [InlineData("1")]
        public async Task CanGetClientsById(string clientStatusId)
        {
            // TODO: Create a new ClientID, Get the Id and then send the request ???
            var httpClient = this.factory.CreateClient();

            // The endpoint or route of the controller action.
            var httpResponse = await httpClient.GetAsync($"{ClientsByIdUri}{clientStatusId}");

            // Must be successful.
            httpResponse.EnsureSuccessStatusCode();

            // Deserialize and examine results.
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var client = JsonConvert.DeserializeObject<ClientStatus>(stringResponse);
            Assert.Equal(clientStatusId, client.Id);
            Assert.Equal(LodeRunner.Core.Models.EntityType.ClientStatus, client.EntityType);
        }
    }
}
