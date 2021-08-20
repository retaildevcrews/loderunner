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
using Ngsa.LodeRunner.DataAccessLayer;
using Ngsa.LodeRunner.Services;

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

            using var l8rService = new LodeRunnerService(config);
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
        internal static IHost BuildWebHost()
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
    }
}
