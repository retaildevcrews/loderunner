// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Ngsa.LodeRunner.Events;

namespace Ngsa.LodeRunner
{
    /// <summary>
    /// Main application class
    /// </summary>
    public sealed partial class App
    {
        /// <summary>
        /// Gets or sets json serialization options
        /// </summary>
        public static JsonSerializerOptions JsonSerializerOptions { get; set; } = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        /// <summary>
        /// Gets or sets cancellation token
        /// </summary>
        public static CancellationTokenSource TokenSource { get; set; } = new CancellationTokenSource();

        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args">Command Line Parameters</param>
        /// <returns>0 on success</returns>
        public static async Task<int> Main(string[] args)
        {
            if (args != null)
            {
                DisplayAsciiArt(args);
            }

            // build the System.CommandLine.RootCommand
            RootCommand root = BuildRootCommand();
            root.Handler = CommandHandler.Create((Config cfg) => App.Run(cfg));

            // run the command handler
            return await root.InvokeAsync(args).ConfigureAwait(false);
        }

        //TODO Move to proper location when merging with DAL
        public static void LogStatusChange(object sender, ClientStatusEventArgs args)
        {
            Console.WriteLine(args.Message); //TODO fix LogStatusChange implementation
        }

        //TODO Move to proper location when merging with DAL
        public static void UpdateCosmosStatus(object sender, ClientStatusEventArgs args)
        {
            // TODO when merging with DAL, need to register delegate to update cosmos
        }

        /// <summary>
        /// System.CommandLine.CommandHandler implementation
        /// </summary>
        /// <param name="config">configuration</param>
        /// <returns>non-zero on failure</returns>
        public static async Task<int> Run(Config config)
        {
            if (config == null)
            {
                Console.WriteLine("CommandOptions is null");
                return -1;
            }

            // set any missing values
            config.SetDefaultValues();

            // don't run the test on a dry run
            if (config.DryRun)
            {
                return DoDryRun(config);
            }

            // create the test
            try
            {
                if (config.DelayStart == -1)
                {
                    ProcessingEventBus.StatusUpdate += UpdateCosmosStatus;
                    ProcessingEventBus.StatusUpdate += LogStatusChange;

                    //TODO change event status to enum, update message
                    ProcessingEventBus.OnStatusUpdate(null, new ClientStatusEventArgs("Initializing", "test init"));

                    LoadSecrets(config);
                    // TODO Initialize DAL

                    ProcessingEventBus.OnStatusUpdate(null, new ClientStatusEventArgs("Ready", "test ready"));
                    try
                    {
                        // wait indefinitely
                        await Task.Delay(config.DelayStart, TokenSource.Token).ConfigureAwait(false);
                    }
                    catch (TaskCanceledException tce)
                    {
                        ProcessingEventBus.OnStatusUpdate(null, new ClientStatusEventArgs("Terminating", tce.Message));
                    }
                    catch (OperationCanceledException oce)
                    {
                        ProcessingEventBus.OnStatusUpdate(null, new ClientStatusEventArgs("Terminating", oce.Message));
                    }
                    finally
                    {
                        ProcessingEventBus.Dispose();
                    }

                    return 1;
                }
                else if (config.DelayStart > 0)
                {
                    Console.WriteLine($"Waiting {config.DelayStart} seconds to start test ...\n");

                    // wait to start the test run
                    await Task.Delay(config.DelayStart * 1000, TokenSource.Token).ConfigureAwait(false);
                }

                ValidationTest lrt = new (config);

                if (config.RunLoop)
                {
                    // build and run the web host
                    IHost host = BuildWebHost();
                    _ = host.StartAsync(TokenSource.Token);

                    // run in a loop
                    int res = lrt.RunLoop(config, TokenSource.Token);

                    // stop and dispose the web host
                    await host.StopAsync(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
                    host.Dispose();
                    host = null;

                    return res;
                }
                else
                {
                    // run one iteration
                    return await lrt.RunOnce(config, TokenSource.Token).ConfigureAwait(false);
                }
            }
            catch (TaskCanceledException tce)
            {
                // log exception
                if (!tce.Task.IsCompleted)
                {
                    Console.WriteLine($"Exception: {tce}");
                    return 1;
                }

                // task is completed
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nException:{ex.Message}");
                return 1;
            }
        }

        /// <summary>
        /// Check to see if the file exists in the current directory
        /// </summary>
        /// <param name="name">file name</param>
        /// <returns>bool</returns>
        public static bool CheckFileExists(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && System.IO.File.Exists(name.Trim());
        }

        // ascii art
        private static void DisplayAsciiArt(string[] args)
        {
            if (!args.Contains("--version") &&
                (args.Contains("-h") ||
                args.Contains("--help") ||
                args.Contains("-d") ||
                args.Contains("--dry-run")))
            {
                const string file = "src/Core/ascii-art.txt";

                try
                {
                    if (File.Exists(file))
                    {
                        string txt = File.ReadAllText(file);

                        Console.ForegroundColor = ConsoleColor.DarkMagenta;
                        Console.WriteLine(txt);
                        Console.ResetColor();
                    }
                }
                catch
                {
                    // ignore any errors
                }
            }
        }

        // build the web host
        private static IHost BuildWebHost()
        {
            // configure the web host builder
            return Host.CreateDefaultBuilder()
                        .ConfigureWebHostDefaults(webBuilder =>
                        {
                            webBuilder.UseStartup<Startup>();
                            webBuilder.UseUrls($"http://*:8080/");
                        })
                        .UseConsoleLifetime()
                        .Build();
        }

        /// <summary>
        /// Load secrets from volume.
        /// </summary>
        /// <param name="config">The configuration.</param>
        private static void LoadSecrets(Config config)
        {
            config.Secrets = Secrets.GetSecretsFromVolume(config.SecretsVolume);

            // set the Cosmos server name for logging
            config.CosmosName = config.Secrets.CosmosServer.Replace("https://", string.Empty, StringComparison.OrdinalIgnoreCase).Replace("http://", string.Empty, StringComparison.OrdinalIgnoreCase);

            int ndx = config.CosmosName.IndexOf('.', StringComparison.OrdinalIgnoreCase);

            if (ndx > 0)
            {
                config.CosmosName = config.CosmosName.Remove(ndx);
            }

            config.CosmosDal = new DataAccessLayer.CosmosDal(config);
        }
    }
}
