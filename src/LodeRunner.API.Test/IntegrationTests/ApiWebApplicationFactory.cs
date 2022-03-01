// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.API.Middleware;
using LodeRunner.API.Test.IntegrationTests.ExecutingTestRun;
using LodeRunner.API.Test.IntegrationTests.Extensions;
using LodeRunner.Core;
using LodeRunner.Core.Interfaces;
using LodeRunner.Data;
using LodeRunner.Data.Interfaces;
using LodeRunner.Services;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace LodeRunner.API.Test.IntegrationTests
{
    /// <summary>
    /// Represents ApiWebApplicationFactory.
    /// </summary>
    /// <typeparam name="TStartup">The type of the startup.</typeparam>
    public class ApiWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
        where TStartup : class
    {
        private readonly ApiPortPoolManager apiPortPoolManager = new ();

        /// <summary>
        /// Gets the next available port.
        /// </summary>
        /// <returns>NextAvailablePort.</returns>
        public int GetNextAvailablePort()
        {
            return this.apiPortPoolManager.GetNextAvailablePort();
        }

        /// <summary>
        /// Creates a <see cref="T:Microsoft.AspNetCore.Hosting.IWebHostBuilder" /> used to set up <see cref="T:Microsoft.AspNetCore.TestHost.TestServer" />.
        /// </summary>
        /// <returns>
        /// A <see cref="T:Microsoft.AspNetCore.Hosting.IWebHostBuilder" /> instance.
        /// </returns>
        /// <remarks>
        /// The default implementation of this method looks for a <c>public static IWebHostBuilder CreateWebHostBuilder(string[] args)</c>
        /// method defined on the entry point of the assembly of <typeparamref name="TEntryPoint" /> and invokes it passing an empty string
        /// array as arguments.
        /// </remarks>
        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<TStartup>()

                // .UseTestServer()
                // .UseUrls($"http://*:{8089}/")
                .UseShutdownTimeout(TimeSpan.FromSeconds(10));
            return builder;
        }

        /// <summary>
        /// Gives a fixture an opportunity to configure the application before it gets built.
        /// </summary>
        /// <param name="builder">The <see cref="T:Microsoft.AspNetCore.Hosting.IWebHostBuilder" /> for the application.</param>
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            Config config = new ();

            config.SecretsVolume = config.SecretsVolume.GetSecretVolume();
            Secrets.LoadSecrets(config);
            CancellationTokenSource cancelTokenSource = new ();

            builder.ConfigureServices(services =>
            {
                services.AddSingleton<CancellationTokenSource>(cancelTokenSource);
                services.AddSingleton<Config>(config);
                services.AddSingleton<ICosmosConfig>(provider => provider.GetRequiredService<Config>());
                services.AddSingleton<CosmosDBSettings>(x => new CosmosDBSettings(x.GetRequiredService<ICosmosConfig>()));
                services.AddSingleton<ICosmosDBSettings>(provider => provider.GetRequiredService<CosmosDBSettings>());
                services.AddTransient<ISettingsValidator>(provider => provider.GetRequiredService<CosmosDBSettings>());

                // Add CosmosDB Repository
                services.AddSingleton<CosmosDBRepository>();
                services.AddSingleton<ICosmosDBRepository, CosmosDBRepository>(provider => provider.GetRequiredService<CosmosDBRepository>());

                services.AddSingleton<TestRunService>();
                services.AddSingleton<ITestRunService>(provider => provider.GetRequiredService<TestRunService>());
            });
        }
    }
}
