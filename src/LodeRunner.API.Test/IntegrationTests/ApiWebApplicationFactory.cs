﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using LodeRunner.API.Core;
using LodeRunner.API.Middleware;
using LodeRunner.Core;
using LodeRunner.Core.Interfaces;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LodeRunner.API.Test.IntegrationTests
{
    /// <summary>
    /// Represents ApiWebApplicationFactory.
    /// </summary>
    /// <typeparam name="TStartup">The type of the startup.</typeparam>
    public class ApiWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
        where TStartup : class
    {
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
            Secrets.LoadSecrets(config);
            CancellationTokenSource cancelTokenSource = new ();

            builder.ConfigureServices(services =>
            {
                services.AddSingleton<CancellationTokenSource>(cancelTokenSource);
                services.AddSingleton<Config>(config);
                services.AddSingleton<ICosmosConfig>(provider => provider.GetRequiredService<Config>());
            });
        }
    }
}