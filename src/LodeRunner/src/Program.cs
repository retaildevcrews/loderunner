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
using LodeRunner.Core.Interfaces;
using LodeRunner.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LodeRunner
{
    /// <summary>
    /// Main application class
    /// </summary>
    public sealed partial class App
    {
        /// <summary>
        /// File path for ascii art display.
        /// </summary>
        public const string AsciiFile = "src/Core/ascii-art.txt";

        /// <summary>
        /// Gets cancellation token
        /// </summary>
        private static CancellationTokenSource cancelTokenSource;

        /// <summary>
        /// Gets or sets json serialization options
        /// </summary>
        public static JsonSerializerOptions JsonSerializerOptions { get; set; } = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args">Command Line Parameters</param>
        /// <returns>0 on success</returns>
        public static async Task<int> Main(string[] args)
        {
            cancelTokenSource = new CancellationTokenSource();

            RegisterTerminationEvents();

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
                return SystemConstants.ExitFail;
            }

            using var l8rService = new LodeRunnerService(config, cancelTokenSource);
            return await l8rService.StartService();
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

        // build the web host
        internal static IHost BuildWebHost(Config config)
        {
            // configure the web host builder
            return Host.CreateDefaultBuilder()
                        .ConfigureWebHostDefaults(webBuilder =>
                        {
                            webBuilder.ConfigureServices(services =>
                            {
                                services.AddSingleton<CancellationTokenSource>(cancelTokenSource);
                                services.AddSingleton<ICosmosConfig>(provider => provider.GetRequiredService<Config>());
                            });
                            webBuilder.UseStartup<Startup>();
                            webBuilder.UseUrls($"http://*:{config.WebHostPort}/");
                        })
                        .UseConsoleLifetime()
                        .Build();
        }

        /// <summary>
        /// Registers the termination events.
        /// </summary>
        private static void RegisterTerminationEvents()
        {
            AppDomain.CurrentDomain.ProcessExit += (s, ev) =>
            {
                if (!cancelTokenSource.IsCancellationRequested)
                {
                    cancelTokenSource.Cancel(false);
                }
            };

            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cancelTokenSource.Cancel(false);
            };
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
                try
                {
                    if (File.Exists(AsciiFile))
                    {
                        string txt = File.ReadAllText(AsciiFile);

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
    }
}
