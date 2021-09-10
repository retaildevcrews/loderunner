// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prometheus;

namespace LodeRunner
{
    /// <summary>
    /// WebHostBuilder Startup
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">the configuration for WebHost</param>
        public Startup(IConfiguration configuration)
        {
            // keep a local reference
            Configuration = configuration;
        }

        /// <summary>
        /// Gets IConfiguration
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Configure the application builder
        /// </summary>
        /// <param name="app">IApplicationBuilder</param>
        /// <param name="life">IHostApplicationLifetime</param>
        public static void Configure(IApplicationBuilder app, IHostApplicationLifetime life)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (life == null)
            {
                throw new ArgumentNullException(nameof(life));
            }

            CancellationTokenSource cancellationTokenSource = app.ApplicationServices.GetRequiredService<CancellationTokenSource>();

            // signal run loop
            life.ApplicationStopping.Register(() =>
            {
                if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
                {
                    cancellationTokenSource.Cancel(false); // TODO: Do we need to pass 'true' to throw and bubble up the exception?
                }
            });

            life.ApplicationStopped.Register(() =>
            {
                Console.WriteLine(JsonSerializer.Serialize(new Dictionary<string, object>
                {
                    { "Date", DateTime.UtcNow },
                    { "EventType", "Shutdown" },
                }));
            });

            // version handler
            app.UseVersion();

            // use routing
            app.UseRouting();

            // map the metrics
            app.UseEndpoints(ep => { ep.MapMetrics(); });
        }
    }
}
