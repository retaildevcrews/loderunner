// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using LodeRunner.API.AutoMapperProfiles;
using LodeRunner.API.Core;
using LodeRunner.API.Handlers.ExceptionMiddleware;
using LodeRunner.API.Middleware;
using LodeRunner.Core;
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
using Microsoft.OpenApi.Models;
using Prometheus;

namespace LodeRunner.API
{
    /// <summary>
    /// WebHostBuilder Startup.
    /// </summary>
    public class Startup
    {
        private const string SwaggerTitle = "LodeRunner.API";
        private static readonly string SwaggerPath = "/swagger/v1/swagger.json";

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
                app.UseCors();
            }

            // Configure Swagger
            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint(SwaggerPath, SwaggerTitle);
                options.RoutePrefix = string.Empty;
            })
            .UseEndpoints(ep =>
                {
                    ep.MapControllers();
                    ep.MapMetrics();
                })
            .UseVersion();
        }

        /// <summary>
        /// Service configuration.
        /// </summary>
        /// <param name="services">The services in the web host.</param>
        public static void ConfigureServices(IServiceCollection services)
        {
            AddSwaggerServices(services);

            int portNumber = AppConfigurationHelper.GetLoadRunnerUIPort(SystemConstants.LodeRunnerUIDefaultPort);
            services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                builder =>
                {
                    builder.WithOrigins(string.Format(SystemConstants.BaseUriLocalHostPort, portNumber), "https://*.githubpreview.dev")
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });

            // set json serialization defaults and api behavior
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });
            services.AddAutoMapper(typeof(LoadTestPayloadProfile));

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

                .AddSingleton<TestRunService>()
                .AddSingleton<ITestRunService>(provider => provider.GetRequiredService<TestRunService>());
        }

        /// <summary>
        /// Add Swagger Services.
        /// </summary>
        /// <param name="services">Service Collection.</param>
        private static void AddSwaggerServices(IServiceCollection services)
        {
            ServiceDescriptor configDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(Config));

            if (configDescriptor == null || configDescriptor.ImplementationInstance == null)
            {
                throw new NullReferenceException("Unable to retrieve Config implementation from IServiceCollection");
            }

            Config config = (Config)configDescriptor.ImplementationInstance;

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = SwaggerTitle, Version = "v1" });

                if (AppConfigurationHelper.IsProductionEnvironment())
                {
                    // NOTE: this xml file documentation is needed to pull example tag from all method's documentation.
                    var filePath = Path.Combine(System.AppContext.BaseDirectory, "LodeRunnerApi.xml");
                    c.IncludeXmlComments(filePath);

                    filePath = Path.Combine(System.AppContext.BaseDirectory, "LodeRunner.Core.xml");
                    c.IncludeXmlComments(filePath);
                }

                c.EnableAnnotations();

                c.DocumentFilter<PathPrefixInsertDocumentFilter>(config.UrlPrefix);
            });

            services.AddSwaggerGenNewtonsoftSupport();
        }
    }
}
