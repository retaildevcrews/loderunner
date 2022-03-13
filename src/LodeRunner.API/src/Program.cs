// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.API.Middleware;
using LodeRunner.Core;
using LodeRunner.Core.Extensions;
using LodeRunner.Core.Interfaces;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LodeRunner.API
{
    /// <summary>
    /// Main application class.
    /// </summary>
    public sealed partial class App
    {
        /// <summary>
        /// File containing ASCII art.
        /// </summary>
        public const string AsciiFile = "ascii-art.txt";

        /// <summary>
        /// Gets cancellation token.
        /// </summary>
        private static CancellationTokenSource cancelTokenSource;

        /// <summary>
        /// The Web Host.
        /// </summary>
        private static IWebHost host = null;

        /// <summary>
        /// The logger application.
        /// </summary>
        private static ILogger logger = null;

        /// <summary>
        /// Gets or sets json serialization options.
        /// </summary>
        public static JsonSerializerOptions JsonSerializerOptions { get; set; } = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args">command line args.</param>
        /// <returns>0 == success.</returns>
        public static async Task<int> Main(string[] args)
        {
            cancelTokenSource = new CancellationTokenSource();

            if (args != null)
            {
                DisplayAsciiArt(args);
            }

            // build the System.CommandLine.RootCommand
            RootCommand root = LRAPICommandLine.BuildRootCommand();
            root.Handler = CommandHandler.Create((Config cfg) => App.RunApp(cfg));

            // run the app
            return await root.InvokeAsync(args).ConfigureAwait(false);
        }

        /// <summary>
        /// Run the app.
        /// </summary>
        /// <param name="config">command line config.</param>
        /// <returns>status.</returns>
        public static async Task<int> RunApp(Config config)
        {
            try
            {
                // This method builds the host and registers related services such as App Logger.
                Init(config);

                if (host == null)
                {
                    return -1;
                }

                // setup sigterm handler
                SetupSigTermHandler(host);

                // start the webserver
                Task hostRun = host.RunAsync();

                // log startup messages
                GetLogger().LogInformation($"LodeRunner.API Backend Started", VersionExtension.Version);

                // this doesn't return except on ctl-c or sigterm
                await hostRun.ConfigureAwait(false);

                // if not canceled, app exit -1
                return cancelTokenSource.IsCancellationRequested ? 0 : -1;
            }
            catch (Exception ex)
            {
                // TODO: Improved the call to LogError to handle InnerExceptions, since this is the Catch at the top level and all exceptions will bubble up to here.

                // end app on error
                GetLogger().LogError(new EventId((int)HttpStatusCode.InternalServerError, nameof(RunApp)), ex, "Exception");

                return -1;
            }
        }

        /// <summary>
        /// Gets the logger from Host services
        /// </summary>
        /// <returns>The App ILogger.</returns>
        private static ILogger GetLogger()
        {
            if (logger == null)
            {
                logger = host.Services.GetRequiredService<ILogger<App>>();
            }

            return logger;
        }

        /// <summary>
        /// Initializes the specified configuration.
        /// </summary>
        /// <param name="config">The configuration.</param>
        private static void Init(Config config)
        {
            // load secrets from volume
            Secrets.LoadSecrets(config);

            // set the logger config
            RequestLogger.CosmosName = config.CosmosName;

            // build the host will register Data Access Services in Startup.
            host = BuildHost(config);
        }

        // Build the web host
        private static IWebHost BuildHost(Config config)
        {
            string projectName = Assembly.GetCallingAssembly().GetName().Name;

            // configure the web host builder
            // configure MinRequestBodyDataRate if required: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/options?view=aspnetcore-6.0#minimum-request-body-data-rate
            IWebHostBuilder builder = WebHost.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<CancellationTokenSource>(cancelTokenSource);
                    services.AddSingleton<Config>(config);
                    services.AddSingleton<ICosmosConfig>(provider => provider.GetRequiredService<Config>());
                })
                .UseUrls($"http://*:{config.Port}/")
                .UseStartup<Startup>()
                .UseShutdownTimeout(TimeSpan.FromSeconds(10))
                .ConfigureLogging(logger =>
                {
                    logger.Setup(logLevelConfig: config, logValues: config, projectName: projectName);
                });

            // build the host
            return builder.Build();
        }

        // Create a CancellationTokenSource that cancels on ctl-c or sigterm
        private static void SetupSigTermHandler(IWebHost host)
        {
            AppDomain.CurrentDomain.ProcessExit += (s, ev) =>
            {
                if (!cancelTokenSource.IsCancellationRequested)
                {
                    cancelTokenSource.Cancel(false);
                }
            };

            Console.CancelKeyPress += async (sender, e) =>
            {
                e.Cancel = true;
                cancelTokenSource.Cancel();

                GetLogger().LogInformation("Shutting Down ...");

                // trigger graceful shutdown for the webhost
                // force shutdown after timeout, defined in UseShutdownTimeout within BuildHost() method
                await host.StopAsync().ConfigureAwait(false);

                // end the app
                Environment.Exit(0);
            };
        }

        /// <summary>
        /// Displays ASCII art for help and dry run executions.
        /// </summary>
        /// <param name="args">CLI arguments.</param>
        private static void DisplayAsciiArt(string[] args)
        {
            if (args != null)
            {
                if (!args.Contains("--version") &&
                    (args.Contains("-h") ||
                    args.Contains("--help") ||
                    args.Contains("-d") ||
                    args.Contains("--dry-run")))
                {
                    try
                    {
                        if (File.Exists(AsciiFile))
                        {
                            string txt = File.ReadAllText(AsciiFile);

                            if (!string.IsNullOrWhiteSpace(txt))
                            {
                                Console.ForegroundColor = ConsoleColor.DarkCyan;
                                Console.WriteLine(txt);
                                Console.ResetColor();
                            }
                        }
                    }
                    catch
                    {
                        // ignore any errors
                    }
                }
            }
        }
    }
}
