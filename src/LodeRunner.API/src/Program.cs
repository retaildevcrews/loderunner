// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.API.Interfaces;
using LodeRunner.API.Middleware;
using LodeRunner.API.Services;
using LodeRunner.Data.ChangeFeed;
using LodeRunner.Data.Interfaces;
using LodeRunner.Services;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Documents.ChangeFeedProcessor.PartitionManagement;
using Microsoft.Extensions.Logging;

namespace LodeRunner.API
{
    /// <summary>
    /// Main application class
    /// </summary>
    public sealed partial class App
    {
        /// <summary>
        /// File containing ASCII art.
        /// </summary>
        public const string AsciiFile = "ascii-art.txt";

        // capture parse errors from env vars
        private static readonly List<string> EnvVarErrors = new ();

        /// <summary>
        /// Gets cancellation token
        /// </summary>
        private static CancellationTokenSource cancelTokenSource;

        /// <summary>
        /// The Web Host
        /// </summary>
        private static IWebHost host = null;

        /// <summary>
        /// Gets or sets json serialization options
        /// </summary>
        public static JsonSerializerOptions JsonSerializerOptions { get; set; } = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        // App configuration values
        public static Config Config { get; } = new ();
        private static IChangeFeedProcessor ChangeFeedProcessor { get; set; }

        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args">command line args</param>
        /// <returns>0 == success</returns>
        public static async Task<int> Main(string[] args)
        {
            if (args != null)
            {
                DisplayAsciiArt(args);
            }

            // build the System.CommandLine.RootCommand
            RootCommand root = BuildRootCommand();
            root.Handler = CommandHandler.Create((Config cfg) => App.RunApp(cfg));

            // run the app
            return await root.InvokeAsync(args).ConfigureAwait(false);
        }

        /// <summary>
        /// Run the app
        /// </summary>
        /// <param name="config">command line config</param>
        /// <returns>status</returns>
        public static async Task<int> RunApp(Config config)
        {
            NgsaLog logger = new () { Name = typeof(App).FullName };

            try
            {
                Init(config);

                if (host == null)
                {
                    return -1;
                }

                // create cache with initial values
                Config.Cache = new Data.Cache(GetClientStatusService(), GetLoadTestConfigService());

                // setup sigterm handler
                App.cancelTokenSource = SetupSigTermHandler(host, logger);

                // start the webserver
                Task hostRun = host.RunAsync();

                // log startup messages
                logger.LogInformation($"LodeRunner.API Backend Started", VersionExtension.Version);

                // start CosmosDB Change Feed Processor
                await GetLRAPIChangeFeedService().StartChangeFeedProcessor(() => EventsSubscription());

                // this doesn't return except on ctl-c or sigterm
                await hostRun.ConfigureAwait(false);

                // if not cancelled, app exit -1
                return cancelTokenSource.IsCancellationRequested ? 0 : -1;
            }
            catch (Exception ex)
            {
                // end app on error
                logger.LogError(nameof(RunApp), "Exception", ex: ex);

                return -1;
            }
        }

        /// <summary>
        /// Registers the events.
        /// </summary>
        private static void EventsSubscription()
        {
            GetLRAPIChangeFeedService().SubscribeToProcessClientStatusChange(ProcessClientStatusChange);

            GetLRAPIChangeFeedService().SubscribeToProcessLoadClientChange(ProcessLoadClientChange);

            GetLRAPIChangeFeedService().SubscribeToProcessLoadTestConfigChange(ProcessLoadTestConfigChange);

            GetLRAPIChangeFeedService().SubscribeToProcessTestRunChange(ProcessTestRunChange);
        }

        /// <summary>
        /// Processes the client status change.
        /// </summary>
        /// <param name="e">The <see cref="ProcessChangesEventArgs"/> instance containing the event data.</param>
        private static void ProcessClientStatusChange(ProcessChangesEventArgs e)
        {
            Config.Cache.ProcessClientStatusChange(e.Document);
        }

        /// <summary>
        /// Processes the load client change.
        /// </summary>
        /// <param name="e">The <see cref="ProcessChangesEventArgs"/> instance containing the event data.</param>
        private static void ProcessLoadClientChange(ProcessChangesEventArgs e)
        {
            // TODO
        }

        /// <summary>
        /// Processes the load test configuration change.
        /// </summary>
        /// <param name="e">The <see cref="ProcessChangesEventArgs"/> instance containing the event data.</param>
        private static void ProcessLoadTestConfigChange(ProcessChangesEventArgs e)
        {
            // TODO
        }

        /// <summary>
        /// Processes the test run change.
        /// </summary>
        /// <param name="e">The <see cref="ProcessChangesEventArgs"/> instance containing the event data.</param>
        private static void ProcessTestRunChange(ProcessChangesEventArgs e)
        {
            // TODO
        }

        /// <summary>
        /// Gets the client status service.
        /// </summary>
        /// <returns>The IClientStatusService</returns>
        private static IClientStatusService GetClientStatusService()
        {
            return (IClientStatusService)host.Services.GetService(typeof(ClientStatusService));
        }

        /// <summary>
        /// Gets the load test configuration service.
        /// </summary>
        /// <returns>The ILoadTestConfigService </returns>
        private static ILoadTestConfigService GetLoadTestConfigService()
        {
            return (ILoadTestConfigService)host.Services.GetService(typeof(LoadTestConfigService));
        }

        /// <summary>
        /// Gets the change feed service.
        /// </summary>
        /// <returns>The ChangeFeed Service.</returns>
        private static ILRAPIChangeFeedService GetLRAPIChangeFeedService()
        {
            return (ILRAPIChangeFeedService)host.Services.GetService(typeof(LRAPIChangeFeedService));
        }

        /// <summary>
        /// Initializes the specified configuration.
        /// </summary>
        /// <param name="config">The configuration.</param>
        private static void Init(Config config)
        {
            // copy command line values
            Config.SetConfig(config);

            // load secrets from volume
            LoadSecrets();

            // set the logger config
            RequestLogger.CosmosName = Config.CosmosName;
            NgsaLog.LogLevel = Config.LogLevel;

            // build the host will register Data Access Services in Startup.
            host = BuildHost();
        }

        // load secrets from volume
        private static void LoadSecrets()
        {
            Config.Secrets = Secrets.GetSecretsFromVolume(Config.SecretsVolume);

            // set the Cosmos server name for logging
            Config.CosmosName = Config.Secrets.CosmosServer.Replace("https://", string.Empty, StringComparison.OrdinalIgnoreCase).Replace("http://", string.Empty, StringComparison.OrdinalIgnoreCase);
            int ndx = Config.CosmosName.IndexOf('.', StringComparison.OrdinalIgnoreCase);
            if (ndx > 0)
            {
                Config.CosmosName = Config.CosmosName.Remove(ndx);
            }
        }

        // Build the web host
        private static IWebHost BuildHost()
        {
            // configure the web host builder
            IWebHostBuilder builder = WebHost.CreateDefaultBuilder()
                .UseUrls($"http://*:{Config.Port}/")
                .UseStartup<Startup>()
                .UseShutdownTimeout(TimeSpan.FromSeconds(10))
                .ConfigureLogging(logger =>
                {
                    // log to XML
                    // this can be replaced when the dotnet XML logger is available
                    logger.ClearProviders();
                    logger.AddNgsaLogger(config => { config.LogLevel = Config.LogLevel; });

                    // if you specify the --log-level option, it will override the appsettings.json options
                    // remove any or all of the code below that you don't want to override
                    if (Config.IsLogLevelSet)
                    {
                        logger.AddFilter("Microsoft", Config.LogLevel)
                        .AddFilter("System", Config.LogLevel)
                        .AddFilter("Default", Config.LogLevel)
                        .AddFilter("LodeRunner.API", Config.LogLevel);
                    }
                });

            // build the host
            return builder.Build();
        }

        // Create a CancellationTokenSource that cancels on ctl-c or sigterm
        private static CancellationTokenSource SetupSigTermHandler(IWebHost host, NgsaLog logger)
        {
            CancellationTokenSource ctCancel = new ();

            Console.CancelKeyPress += async (sender, e) =>
            {
                e.Cancel = true;
                ctCancel.Cancel();

                logger.LogInformation("Shutdown", "Shutting Down ...");

                // trigger graceful shutdown for the webhost
                // force shutdown after timeout, defined in UseShutdownTimeout within BuildHost() method
                await host.StopAsync().ConfigureAwait(false);

                // end the app
                Environment.Exit(0);
            };

            return ctCancel;
        }

        /// <summary>
        /// Displays ASCII art for help and dry run executions.
        /// </summary>
        /// <param name="args">CLI arguments</param>
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
