// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using LodeRunner.API.Data;
using LodeRunner.API.Handlers.ExceptionMiddleware;
using LodeRunner.API.Interfaces;
using LodeRunner.API.Middleware;
using LodeRunner.API.Services;
using LodeRunner.Core.Interfaces;
using LodeRunner.Data;
using LodeRunner.Data.Interfaces;
using LodeRunner.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace LodeRunner.API
{
    /// <summary>
    /// WebHostBuilder Startup.
    /// </summary>
    public class Startup
    {
        private const string SwaggerTitle = "LodeRunner.API";
        private static string swaggerPath = "/swagger.json";

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">the configuration for WebHost.</param>
        public Startup(IConfiguration configuration)
        {
            // keep a local reference
            this.Configuration = configuration;
        }

        /// <summary>
        /// Gets IConfiguration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Configure the application builder.
        /// </summary>
        /// <param name="app">IApplicationBuilder.</param>
        /// <param name="env">IWebHostEnvironment.</param>
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (env == null)
            {
                throw new ArgumentNullException(nameof(env));
            }

            Config config = app.ApplicationServices.GetRequiredService<Config>();

            // log http responses to the console
            // this should be first as it "wraps" all requests
            if (config.LogLevel != LogLevel.None)
            {
                app.UseRequestLogger(config, new RequestLoggerOptions
                {
                    Log2xx = config.LogLevel <= LogLevel.Information,
                    Log3xx = config.LogLevel <= LogLevel.Information,
                    Log4xx = config.LogLevel <= LogLevel.Warning,
                    Log5xx = true,
                });
            }

            app.ConfigureCustomExceptionMiddleware();

            // UseHsts in prod
            if (env.IsProduction())
            {
                app.UseHsts();
            }

            // redirect /
            app.Use(async (context, next) =>
            {
                // matches /
                if (context.Request.Path.Equals("/"))
                {
                    // return the version info
                    context.Response.Redirect($"{config.UrlPrefix}/index.html", true);
                    return;
                }
                else
                {
                    // not a match, so call next middleware handler
                    await next().ConfigureAwait(false);
                }
            });

            // add middleware handlers
            app.UseRouting();

            if (env.IsDevelopment())
            {
                app.UseCors(builder =>
                {
                    builder.WithOrigins("http://localhost:3000")
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            }

            app.UseEndpoints(ep =>
            {
                ep.MapControllers();
                ep.MapMetrics();
            })
            .UseSwaggerUI(c =>
            {
                if (!string.IsNullOrEmpty(config.UrlPrefix))
                {
                    swaggerPath = config.UrlPrefix + swaggerPath;
                }

                c.SwaggerEndpoint(swaggerPath, SwaggerTitle);
                c.RoutePrefix = string.Empty;
            })
            .UseSwaggerReplaceJson("swagger.json", config.UrlPrefix)
            .UseVersion();
        }

        /// <summary>
        /// Service configuration.
        /// </summary>
        /// <param name="services">The services in the web host.</param>
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            // set json serialization defaults and api behavior
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.IgnoreNullValues = true;
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            services
                .AddSingleton<CosmosDBSettings>(x => new CosmosDBSettings(x.GetRequiredService<ICosmosConfig>()))
                .AddSingleton<ICosmosDBSettings>(provider => provider.GetRequiredService<CosmosDBSettings>())
                .AddTransient<ISettingsValidator>(provider => provider.GetRequiredService<CosmosDBSettings>())

                // Add CosmosDB Repository
                .AddSingleton<CosmosDBRepository>()
                .AddSingleton<ICosmosDBRepository, CosmosDBRepository>(provider => provider.GetRequiredService<CosmosDBRepository>())

                // Add Services
                .AddSingleton<ClientStatusService>()
                .AddSingleton<IClientStatusService>(provider => provider.GetRequiredService<ClientStatusService>())

                .AddSingleton<LoadTestConfigService>()
                .AddSingleton<ILoadTestConfigService>(provider => provider.GetRequiredService<LoadTestConfigService>())

                .AddSingleton<LRAPIChangeFeedService>()
                .AddSingleton<ILRAPIChangeFeedService>(provider => provider.GetRequiredService<LRAPIChangeFeedService>())

                .AddSingleton<LRAPICache>()
                .AddSingleton<ILRAPICache>(provider => provider.GetRequiredService<LRAPICache>());
        }
    }
}
