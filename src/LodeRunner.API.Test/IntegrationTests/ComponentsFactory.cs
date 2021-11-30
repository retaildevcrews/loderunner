// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.API.Services;
using LodeRunner.Core;
using LodeRunner.Core.CommandLine;
using LodeRunner.Core.Models;
using LodeRunner.Services;
using Xunit;

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
            InitializeSystemComponents(apiWebFactory.Services);
            return httpClient;
        }

        /// <summary>
        /// CreateAndStartLodeRunnerService Instance.
        /// </summary>
        /// <param name="callerName">The caller Name.</param>
        /// <returns>LodeRunnerService.</returns>
        public static async Task<LodeRunnerService> CreateAndStartLodeRunnerServiceInstance(string callerName)
        {
            string dateTimeUnique = $"{DateTime.UtcNow:yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK}";
            var args = new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--delay-start", "-1", "--secrets-volume", "secrets", "--region", $"IntegrationTesting-{callerName}-{dateTimeUnique}" };

            LodeRunner.Config lrConfig = new ();
            RootCommand root = LRCommandLine.BuildRootCommand();

            LodeRunnerService l8rService = null;

            // Create lrConfig from arguments
            root.Handler = CommandHandler.Create<LodeRunner.Config>((lrConfig) =>
            {
                Assert.NotNull(lrConfig);
                Assert.True(lrConfig.Server.Count > 0);

                // TODO: Validate every argument not just servers ??

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
        /// Initialize SystemComponents.
        /// </summary>
        /// <param name="serviceProvider">the ServiceProvider.</param>
        private static void InitializeSystemComponents(IServiceProvider serviceProvider)
        {
            ISystemComponentsService systemComponentsService = (ISystemComponentsService)serviceProvider.GetService(typeof(SystemComponentsService));

            systemComponentsService.InitializeSystemComponents();
        }
    }
}
