// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.Core;
using LodeRunner.Core.CommandLine;
using LodeRunner.Core.Extensions;
using LodeRunner.Core.Interfaces;
using LodeRunner.Interfaces;
using LodeRunner.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LodeRunner
{
    /// <summary>
    /// Main application class.
    /// </summary>
    public sealed partial class App
    {
        /// <summary>
        /// File path for ascii art display.
        /// </summary>
        public const string AsciiFile = "src/Core/ascii-art.txt";

        /// <summary>
        /// The project name.
        /// </summary>
        internal static readonly string ProjectName = Assembly.GetCallingAssembly().GetName().Name;

        /// <summary>
        /// Gets cancellation token.
        /// </summary>
        private static readonly CancellationTokenSource CancelTokenSource = new ();

        /// <summary>
        /// Private constructor to prevent instantiation.
        /// </summary>
        private App() { }

        /// <summary>
        /// Gets or sets json serialization options.
        /// </summary>
        /// <returns>JsonSerializerOptions.</returns>
        public static JsonSerializerOptions JsonSerializerOptions { get; set; } = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args">Command Line Parameters.</param>
        /// <returns>0 on success.</returns>
        public static async Task<int> Main(string[] args)
        {
            RegisterTerminationEvents();

            if (args != null)
            {
                DisplayAsciiArt(args);
            }

            // build the System.CommandLine.RootCommand
            RootCommand root = LRCommandLine.GetRootCommand(args);

            root.Handler = CommandHandler.Create((Config cfg) => App.Run(cfg, root.Name == SystemConstants.LodeRunnerClientMode));

            // run the command handler
            return await root.InvokeAsync(args).ConfigureAwait(false);
        }

        /// <summary>
        /// System.CommandLine.CommandHandler implementation.
        /// </summary>
        /// <param name="config">configuration.</param>
        /// <param name="isClientMode">determines if is ClientMode.</param>
        /// <returns>non-zero on failure.</returns>
        public static async Task<int> Run(Config config, bool isClientMode)
        {
            if (config == null)
            {
                // Note: At this point of time the ILogger has not been created yet, so we use Console.
                Console.WriteLine("CommandOptions is null");
                return Core.SystemConstants.ExitFail;
            }

            //Note: config.IsClientMode does not get auto-populated by RootCommand since it does not get parsed, so we need to set it manually.
            config.IsClientMode = isClientMode;

            var services = new ServiceCollection();

            // Register Services and System Objects.
            ConfigureServicesAndSystemObjects(services, config);

            using var serviceProvider = services.BuildServiceProvider();

            using var l8rService = serviceProvider.GetService<LodeRunnerService>();

            return await l8rService.StartService();
        }

        /// <summary>
        /// Check to see if the file exists in the current directory.
        /// </summary>
        /// <param name="name">file name.</param>
        /// <returns>bool.</returns>
        public static bool CheckFileExists(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && System.IO.File.Exists(name.Trim());
        }

        /// <summary>
        /// Configure App Services.
        /// </summary>
        /// <param name="services">Service Collection.</param>
        /// <param name="config">The config.</param>
        private static void ConfigureServicesAndSystemObjects(ServiceCollection services, Config config)
        {
            services.AddLogging(logger =>
                {
                    logger.Setup(config, ProjectName);
                })
                .AddSingleton<Config>(config)
                .AddSingleton<CancellationTokenSource>(CancelTokenSource)
                .AddSingleton<LodeRunnerService>()
                .AddSingleton<ILodeRunnerService>(provider => provider.GetRequiredService<LodeRunnerService>());
        }

        /// <summary>
        /// Registers the termination events.
        /// </summary>
        private static void RegisterTerminationEvents()
        {
            AppDomain.CurrentDomain.ProcessExit += (s, ev) =>
            {
                if (!CancelTokenSource.IsCancellationRequested)
                {
                    CancelTokenSource.Cancel(false);
                }
            };

            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                CancelTokenSource.Cancel(false);
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

                        // Note: we use Console for Ascii Art.
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
