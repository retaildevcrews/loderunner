// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using Ngsa.Middleware.CommandLine;

namespace Ngsa.LodeRunner
{
    /// <summary>
    /// Main application class
    /// </summary>
    public sealed partial class App
    {
        /// <summary>
        /// Build the RootCommand for parsing
        /// </summary>
        /// <returns>RootCommand</returns>
        public static RootCommand BuildRootCommand()
        {
            RootCommand root = new RootCommand
            {
                Name = "LodeRunner",
                Description = "Validate API responses",
                TreatUnmatchedTokensAsErrors = true,
            };

            root.AddOption(new Option<List<string>>(new string[] { "-s", "--server" }, Parsers.ParseStringList, true, "Server(s) to test"));
            root.AddOption(new Option<List<string>>(new string[] { "-f", "--files" }, Parsers.ParseStringList, true, "List of files to test"));
            root.AddOption(new Option<string>(new string[] { "--zone" }, Parsers.ParseString, true, "Zone for logging"));
            root.AddOption(new Option<string>(new string[] { "--region" }, Parsers.ParseString, true, "Region for logging"));
            root.AddOption(new Option<bool>(new string[] { "-p", "--prometheus" }, Parsers.ParseBool, true, "Send metrics to Prometheus"));
            root.AddOption(new Option<string>(new string[] { "--tag" }, Parsers.ParseString, true, "Tag for logging"));
            root.AddOption(new Option<int>(new string[] { "-l", "--sleep" }, Parsers.ParseIntGTZero, true, "Sleep (ms) between each request"));
            root.AddOption(new Option<bool>(new string[] { "-j", "--strict-json" }, Parsers.ParseBool, true, "Use strict json when parsing"));
            root.AddOption(new Option<string>(new string[] { "-u", "--base-url" }, Parsers.ParseString, true, "Base url for files"));
            root.AddOption(new Option<bool>(new string[] { "-v", "--verbose" }, Parsers.ParseBool, true, "Display verbose results"));
            root.AddOption(new Option<bool>(new string[] { "-r", "--run-loop" }, Parsers.ParseBool, true, "Run test in an infinite loop"));
            root.AddOption(new Option<bool>(new string[] { "--verbose-errors" }, Parsers.ParseBool, true, "Log verbose error messages"));
            root.AddOption(new Option<bool>(new string[] { "--random" }, Parsers.ParseBool, true, "Run requests randomly (requires --run-loop)"));
            root.AddOption(new Option<int>(new string[] { "--duration" }, Parsers.ParseIntGTZero, true, "Test duration (seconds)  (requires --run-loop)"));
            root.AddOption(new Option<int>(new string[] { "--summary-minutes" }, Parsers.ParseIntGTZero, true, "Display summary results (minutes)  (requires --run-loop)"));
            root.AddOption(new Option<int>(new string[] { "-t", "--timeout" }, Parsers.ParseIntGTZero, true, "Request timeout (seconds)"));
            root.AddOption(new Option<int>(new string[] { "--max-concurrent" }, Parsers.ParseIntGTZero, true, "Max concurrent requests"));
            root.AddOption(new Option<int>(new string[] { "--max-errors" }, Parsers.ParseIntGTZero, true, "Max validation errors"));
            root.AddOption(new Option<int>(new string[] { "--delay-start" }, Parsers.ParseIntGTZero, true, "Delay test start (seconds)"));
            root.AddOption(new Option<bool>(new string[] { "-d", "--dry-run" }, "Validates configuration"));

            // these require access to --run-loop so are added at the root level
            root.AddValidator(ValidateRunLoopDependencies);

            return root;
        }

        // validate --duration and --random based on --run-loop
        private static string ValidateRunLoopDependencies(CommandResult result)
        {
            OptionResult runLoopRes = result.Children.FirstOrDefault(c => c.Symbol.Name == "run-loop") as OptionResult;
            OptionResult durationRes = result.Children.FirstOrDefault(c => c.Symbol.Name == "duration") as OptionResult;
            OptionResult randomRes = result.Children.FirstOrDefault(c => c.Symbol.Name == "random") as OptionResult;

            bool runLoop = runLoopRes.GetValueOrDefault<bool>();
            int? duration = durationRes.GetValueOrDefault<int?>();
            bool random = randomRes.GetValueOrDefault<bool>();

            if (duration != null && duration > 0 && !runLoop)
            {
                return "--run-loop must be true to use --duration";
            }

            if (random && !runLoop)
            {
                return "--run-loop must be true to use --random";
            }

            return string.Empty;
        }

        // handle --dry-run
        private static int DoDryRun(Config config)
        {
            DisplayAsciiArt();

            // display the config
            Console.WriteLine("dry run");
            Console.WriteLine($"   Server          {config.Server}");
            Console.WriteLine($"   Files (count)   {config.Files.Count}");

            if (!string.IsNullOrWhiteSpace(config.Zone))
            {
                Console.WriteLine($"   Zone            {config.Zone}");
            }

            if (!string.IsNullOrWhiteSpace(config.Region))
            {
                Console.WriteLine($"   Region          {config.Region}");
            }

            if (!string.IsNullOrWhiteSpace(config.Tag))
            {
                Console.WriteLine($"   Tag             {config.Tag}");
            }

            Console.WriteLine($"   Run Loop        {config.RunLoop}");
            Console.WriteLine($"   Sleep           {config.Sleep}");
            Console.WriteLine($"   Prometheus      {config.Prometheus}");
            Console.WriteLine($"   Verbose Errors  {config.VerboseErrors}");
            Console.WriteLine($"   Strict Json     {config.StrictJson}");
            Console.WriteLine($"   Duration        {config.Duration}");
            Console.WriteLine($"   Delay Start     {config.DelayStart}");
            Console.WriteLine($"   Max Concurrent  {config.MaxConcurrent}");
            Console.WriteLine($"   Max Errors      {config.MaxErrors}");
            Console.WriteLine($"   Random          {config.Random}");
            Console.WriteLine($"   Timeout         {config.Timeout}");
            Console.WriteLine($"   Verbose         {config.Verbose}");

            return 0;
        }

        // Display the ASCII art file if it exists
        private static void DisplayAsciiArt()
        {
            const string file = "src/core/ascii-art.txt";

            if (File.Exists(file))
            {
                Console.WriteLine(File.ReadAllText(file));
            }
        }
    }
}
