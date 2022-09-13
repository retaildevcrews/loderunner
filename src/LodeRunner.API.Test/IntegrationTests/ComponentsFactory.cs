// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.API.Test.IntegrationTests.Extensions;
using LodeRunner.Core;
using LodeRunner.Core.CommandLine;
using LodeRunner.Services;
using Xunit;
using LRLoggingExtensions = LodeRunner.Core.Extensions.LoggingBuilderExtensions;

namespace LodeRunner.API.Test.IntegrationTests
{
    /// <summary>
    /// Represents the Components Factory.
    /// </summary>
    public static class ComponentsFactory
    {
        /// <summary>
        /// Creates a new HttpClient using the WebApp factory.
        /// </summary>
        /// <param name="apiWebFactory">The API web factory.</param>
        /// <returns>the HttpClient.</returns>
        public static HttpClient CreateLodeRunnerAPIHttpClient(ApiWebApplicationFactory<Startup> apiWebFactory)
        {
            var httpClient = apiWebFactory.CreateClient();
            return httpClient;
        }

        /// <summary>
        /// Get TestRun Service Single using the WebApp factory.
        /// </summary>
        /// <param name="apiWebFactory">The API web factory.</param>
        /// <returns>the HttpClient.</returns>
        public static TestRunService GetTestRunService(ApiWebApplicationFactory<Startup> apiWebFactory)
        {
            var svc = apiWebFactory.Services.GetService(typeof(TestRunService)) as TestRunService;
            return svc;
        }

        /// <summary>
        /// CreateAndStartLodeRunnerService Instance.
        /// </summary>
        /// <param name="callerName">The caller Name.</param>
        /// <returns>LodeRunnerService.</returns>
        public static async Task<LodeRunnerService> CreateAndStartLodeRunnerServiceInstance(string callerName)
        {
            string uniqueRegion = $"IntegrationTesting-{callerName}-{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}";

            string secrets = "secrets".GetSecretVolume();

            var args = new string[] { "--mode", "Client", "--secrets-volume", $"{secrets}", "--region", uniqueRegion };

            LodeRunner.Config lrConfig = new();
            RootCommand rootClient = LRCommandLine.BuildRootClientMode();

            LodeRunnerService l8rService = null;

            // Create lrConfig from arguments
            rootClient.Handler = CommandHandler.Create<LodeRunner.Config>((lrConfig) =>
            {
                Assert.NotNull(lrConfig);
                lrConfig.IsClientMode = rootClient.Name == LodeRunner.Core.SystemConstants.LodeRunnerClientMode;

                Assert.True(lrConfig.IsClientMode, "Incorrect LodeRunner Root Command was created.");
                Assert.StartsWith(uniqueRegion, lrConfig.Region);

                // Initialize and Start LodeRunner Service
                Secrets.LoadSecrets(lrConfig);
                CancellationTokenSource cancelTokenSource = new();

                var logger = LRLoggingExtensions.CreateLogger<LodeRunnerService>(logLevelConfig: lrConfig, logValues: lrConfig);
                l8rService = new LodeRunnerService(lrConfig, cancelTokenSource, logger);

                Assert.NotNull(l8rService);

                _ = l8rService.StartService();
            });

            await rootClient.InvokeAsync(args).ConfigureAwait(true);

            return l8rService;
        }
    }
}
