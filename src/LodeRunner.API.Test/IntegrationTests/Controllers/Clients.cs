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
using LodeRunner.Core;
using LodeRunner.Core.CommandLine;
using LodeRunner.Core.Extensions;
using LodeRunner.Core.Models;
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
            string dateTimeUnique = $"{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}";
            var validArgs = new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--delay-start", "-1", "--secrets-volume", "secrets", "--region", $"IntegrationTesting-{dateTimeUnique}" };

            using var l8rService = await CreateAndStartLodeRunnerServiceInstance(validArgs);

            string clientStatusId = l8rService.ClientStatusId;

            Assert.False(string.IsNullOrEmpty(clientStatusId), "Unable to retrieve ClientStatusId.");

            var httpClient = this.factory.CreateClient();

            //var httpResponse = await SendGetByIdRequest(httpClient, ClientsByIdUri, clientStatusId);
            //this.ValidateResponse(httpResponse, clientStatusId, ClientStatusType.Ready);

            await this.WaitAndValidateStatus(httpClient, ClientsByIdUri, clientStatusId, ClientStatusType.Ready);

            l8rService.StopService();

            await this.WaitAndValidateStatus(httpClient, ClientsByIdUri, clientStatusId, ClientStatusType.Terminating);
        }

        /// <summary>
        /// CreateAndStartLodeRunnerService Instance.
        /// </summary>
        /// <param name="args">Start arguments.</param>
        /// <returns>LodeRunnerService.</returns>
        private static async Task<LodeRunnerService> CreateAndStartLodeRunnerServiceInstance(string[] args)
        {
            LodeRunner.Config lrConfig = new ();
            RootCommand root = LRCommandLine.BuildRootCommand();

            LodeRunnerService l8rService = null;

            // Create lrConfig from arguments
            root.Handler = CommandHandler.Create<LodeRunner.Config>((lrConfig) =>
            {
                Assert.NotNull(lrConfig);
                Assert.True(lrConfig.Server.Count > 0);

                // TODO: Validate every argument not just servers

                // Initialize and Start LodeRunner Service
                Secrets.LoadSecrets(lrConfig);
                CancellationTokenSource cancelTokenSource = new ();
                l8rService = new LodeRunnerService(lrConfig, cancelTokenSource);

                Assert.NotNull(l8rService);

                _ = l8rService.StartService();
            });

            await root.InvokeAsync(args).ConfigureAwait(true);

            return l8rService;
        }

        /// <summary>
        /// Send GetById Request.
        /// </summary>
        /// <param name="httpClient">the httpClient.</param>
        /// <param name="clientsByIdUri">the clientsById Uri.</param>
        /// <param name="clientStatusId">the clientStatusId.</param>
        /// <returns>HttpResponseMessage.</returns>
        private async Task<HttpResponseMessage> SendGetByIdRequest(HttpClient httpClient, string clientsByIdUri, string clientStatusId)
        {
            HttpResponseMessage httpResponse = null;

            int timeout = WaitingTimeIncrementMs;
            while (timeout <= 30000)
            {
                await Task.Delay(WaitingTimeIncrementMs);
                httpResponse = await httpClient.GetAsync($"{clientsByIdUri}{clientStatusId}");

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

            return httpResponse;
        }

        /// <summary>
        /// ValidateResponse based on ClientStatusType.
        /// </summary>
        /// <param name="httpResponse">The httpResponse.</param>
        /// <param name="clientStatusId">The clientStatusId.</param>
        /// <param name="clientStatusType">The clientStatusType.</param>
        private void ValidateResponse(HttpResponseMessage httpResponse, string clientStatusId, ClientStatusType clientStatusType)
        {
            if (httpResponse != null && httpResponse.IsSuccessStatusCode)
            {
                var client = httpResponse.Content.ReadFromJsonAsync<Client>(this.jsonOptions);

                Assert.True(client.Result != null);

                Assert.Equal(clientStatusId, client.Result.ClientStatusId);
                Assert.Equal(EntityType.Client, client.Result.EntityType);

                Assert.Equal(clientStatusType, client.Result.Status);
            }
            else
            {
                Assert.True(false, $"Unable to process request for ClientStatusId: {clientStatusId}\tClientStatusType: '{clientStatusType}' - HttpStatusCode:{httpResponse.StatusCode}-{httpResponse.ReasonPhrase}");
            }
        }

        /// <summary>
        /// ValidateResponse based on ClientStatusType.
        /// </summary>
        /// <param name="httpClient">the httpClient.</param>
        /// <param name="clientsByIdUri">clientsById Uri.</param>
        /// <param name="clientStatusId">clientStatusId.</param>
        /// <param name="clientStatusType">The clientStatusType.</param>
        private async Task<bool> WaitAndValidateStatus(HttpClient httpClient, string clientsByIdUri, string clientStatusId, ClientStatusType clientStatusType)
        {
            int timeout = WaitingTimeIncrementMs;

            bool found = false;

            while (timeout <= 30000)
            {
                await Task.Delay(WaitingTimeIncrementMs);

                var httpResponse = await this.SendGetByIdRequest(httpClient, clientsByIdUri, clientStatusId);

                if (httpResponse != null && httpResponse.IsSuccessStatusCode)
                {
                    var client = httpResponse.Content.ReadFromJsonAsync<Client>(this.jsonOptions);

                    found = client.Result != null && client.Result.Status == clientStatusType;

                    if (found)
                    {
                        break;
                    }

                    await Task.Delay(WaitingTimeIncrementMs);
                    timeout += WaitingTimeIncrementMs;
                }
            }

            return found;
        }

    }
}
