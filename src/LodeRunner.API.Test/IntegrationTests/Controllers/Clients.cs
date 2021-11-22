// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.API.Models;
using LodeRunner.Core;
using LodeRunner.Core.CommandLine;
using LodeRunner.Services;
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
        private const int WaitingTimeIncrementMs = 5000;

        private readonly ApiWebApplicationFactory<Startup> factory;

        private readonly JsonSerializerOptions jsonOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="Clients"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        public Clients(ApiWebApplicationFactory<Startup> factory)
        {
            this.factory = factory;

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
            var httpClient = this.factory.CreateClient();

            var httpResponse = await httpClient.GetAsync(ClientsUri);

            Assert.False(httpResponse.StatusCode == HttpStatusCode.NoContent, "Response Code 204 - No Content.");

            if (httpResponse.IsSuccessStatusCode)
            {
                var clients = await httpResponse.Content.ReadFromJsonAsync<IEnumerable<Client>>(this.jsonOptions);

                Assert.True(clients != null);

                Assert.True(clients.Any(), "Cannot get any Clients");
            }
            else
            {
                Assert.True(false, $"Unable to process request - {httpResponse}");
            }
        }

        /// <summary>
        /// Determines whether this instance [can get clients by identifier] the specified client status identifier.
        /// </summary>
        /// <returns><see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        [Trait("Category", "Integration")]
        public async Task CanGetClientsById()
        {
            // Map CommandLine arguments.
            var validArgs = new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--delay-start", "-1", "--secrets-volume", "secrets" , "--region" , "IntegrationTesting" };

            LodeRunner.Config lrConfig = new ();
            RootCommand root = LRCommandLine.BuildRootCommand();

            string clientStatusId = string.Empty;

            // Create lrConfig from arguments
            root.Handler = CommandHandler.Create<LodeRunner.Config>((lrConfig) =>
            {
                Assert.NotNull(lrConfig);
                Assert.True(lrConfig.Server.Count > 0);

                // TODO: Validate every argument not just servers

                // Initialize and Start LodeRunner Service
                Secrets.LoadSecrets(lrConfig);
                CancellationTokenSource cancelTokenSource = new ();
                using var l8rService = new LodeRunnerService(lrConfig, cancelTokenSource);
                _ = l8rService.StartService();

                clientStatusId = l8rService.ClientStatusId;
            });

            await root.InvokeAsync(validArgs).ConfigureAwait(false);

            Assert.False(string.IsNullOrEmpty(clientStatusId), "Unable to retrieve ClientStatusId.");

            var httpClient = this.factory.CreateClient();

            // TODO: how to check for Terminating status

            int timeout = WaitingTimeIncrementMs;
            HttpResponseMessage httpResponse = null;
            while (timeout < 30000)
            {
                await Task.Delay(WaitingTimeIncrementMs);
                httpResponse = await httpClient.GetAsync($"{ClientsByIdUri}{clientStatusId}");
                Assert.False(httpResponse.StatusCode == HttpStatusCode.NoContent, "Response Code 204 - No Content.");

                if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    await Task.Delay(WaitingTimeIncrementMs);
                    timeout += WaitingTimeIncrementMs;
                }
                else if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    break;
                }
            }

            if (httpResponse != null && httpResponse.IsSuccessStatusCode)
            {
                var client = await httpResponse.Content.ReadFromJsonAsync<Client>(this.jsonOptions);

                Assert.True(client != null);

                Assert.Equal(clientStatusId, client.ClientStatusId);
                Assert.Equal(LodeRunner.Core.Models.EntityType.Client, client.EntityType);
            }
            else
            {
                Assert.True(false, $"Unable to process request - {httpResponse.ReasonPhrase}");
            }

        }
    }
}
