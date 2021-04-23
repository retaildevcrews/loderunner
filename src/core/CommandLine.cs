// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;
using Ngsa.Middleware.CommandLine;

namespace Ngsa.LodeRunner
{
    public enum LogFormat
    {
        Json, Tsv, None
    }

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

            root.AddOption(new Option<List<string>>(new string[] { "--server", "-s" }, Parsers.ParseStringList, true, "Server(s) to test"));
            root.AddOption(new Option<List<string>>(new string[] { "--files", "-f" }, Parsers.ParseStringList, true, "List of files to test"));
            root.AddOption(new Option<string>(new string[] { "--base-url", "-u" }, Parsers.ParseString, true, "Base url for files"));
            root.AddOption(new Option<int>(new string[] { "--delay-start" }, Parsers.ParseIntGTZero, true, "Delay test start (seconds)"));
            root.AddOption(new Option<int>(new string[] { "--duration" }, Parsers.ParseIntGTZero, true, "Test duration (seconds)  (requires --run-loop)"));
            root.AddOption(new Option<LogFormat>(new string[] { "--log-format" }, "Log Format"));
            root.AddOption(new Option<int>(new string[] { "--max-concurrent" }, Parsers.ParseIntGTZero, true, "Max concurrent requests"));
            root.AddOption(new Option<int>(new string[] { "--max-errors" }, Parsers.ParseIntGTZero, true, "Max validation errors"));
            root.AddOption(new Option<bool>(new string[] { "--prometheus", "-p" }, Parsers.ParseBool, true, "Send metrics to Prometheus"));
            root.AddOption(new Option<string>(new string[] { "--region" }, Parsers.ParseString, true, "Region for logging"));
            root.AddOption(new Option<bool>(new string[] { "--random" }, Parsers.ParseBool, true, "Run requests randomly (requires --run-loop)"));
            root.AddOption(new Option<bool>(new string[] { "--run-loop", "-r" }, Parsers.ParseBool, true, "Run test in an infinite loop"));
            root.AddOption(new Option<int>(new string[] { "--sleep", "-l" }, Parsers.ParseIntGTZero, true, "Sleep (ms) between each request"));
            root.AddOption(new Option<bool>(new string[] { "--strict-json", "-j" }, Parsers.ParseBool, true, "Use strict json when parsing"));
            root.AddOption(new Option<string>(new string[] { "--tag" }, Parsers.ParseString, true, "Tag for logging"));
            root.AddOption(new Option<int>(new string[] { "--timeout", "-t" }, Parsers.ParseIntGTZero, true, "Request timeout (seconds)"));
            root.AddOption(new Option<bool>(new string[] { "--verbose", "-v" }, Parsers.ParseBool, true, "Display verbose results"));
            root.AddOption(new Option<bool>(new string[] { "--verbose-errors" }, Parsers.ParseBool, true, "Log verbose error messages"));
            root.AddOption(new Option<string>(new string[] { "--webv-prefix" }, () => "https://", "Prefix for server URLs"));
            root.AddOption(new Option<string>(new string[] { "--webv-suffix" }, () => ".azurewebsites.net", "Suffix for server URLs"));
            root.AddOption(new Option<bool>(new string[] { "--xml-summary", "-x" }, "Display test summary in XML format"));
            root.AddOption(new Option<string>(new string[] { "--zone" }, Parsers.ParseString, true, "Zone for logging"));
            root.AddOption(new Option<bool>(new string[] { "--dry-run", "-d" }, "Validates configuration"));
            root.AddOption(new Option<bool>(new string[] { "--version" }, "Show version information"));

            // these require access to --run-loop so are added at the root level
            root.AddValidator(ValidateRunLoopDependencies);

            return root;
        }

        // validate --duration and --random based on --run-loop
        private static string ValidateRunLoopDependencies(CommandResult result)
        {
            string errors = string.Empty;

            OptionResult runLoopRes = result.Children.FirstOrDefault(c => c.Symbol.Name == "run-loop") as OptionResult;
            OptionResult durationRes = result.Children.FirstOrDefault(c => c.Symbol.Name == "duration") as OptionResult;
            OptionResult randomRes = result.Children.FirstOrDefault(c => c.Symbol.Name == "random") as OptionResult;
            OptionResult promRes = result.Children.FirstOrDefault(c => c.Symbol.Name == "prometheus") as OptionResult;

            bool runLoop = runLoopRes.GetValueOrDefault<bool>();
            int? duration = durationRes.GetValueOrDefault<int?>();
            bool random = randomRes.GetValueOrDefault<bool>();
            bool prometheus = promRes.GetValueOrDefault<bool>();

            if (duration != null && duration > 0 && !runLoop)
            {
                errors += "--run-loop must be true to use --duration\n";
            }

            if (random && !runLoop)
            {
                errors += "--run-loop must be true to use --random\n";
            }

            if (prometheus && !runLoop)
            {
                errors += "--run-loop must be true to use --prometheus\n";
            }

            return errors;
        }

        // handle --dry-run
        private static int DoDryRun(Config config)
        {
            // display the config
            Console.WriteLine("dry run");
            Console.WriteLine($"   Server          {string.Join(' ', config.Server)}");
            Console.WriteLine($"   Files           {string.Join(' ', config.Files)}");

            if (!string.IsNullOrWhiteSpace(config.Region))
            {
                Console.WriteLine($"   Region          {config.Region}");
            }

            if (!string.IsNullOrWhiteSpace(config.Zone))
            {
                Console.WriteLine($"   Zone            {config.Zone}");
            }

            Console.WriteLine($"   Delay Start     {config.DelayStart}");
            Console.WriteLine($"   Duration        {config.Duration}");
            Console.WriteLine($"   Log Format      {config.LogFormat}");
            Console.WriteLine($"   Max Concurrent  {config.MaxConcurrent}");
            Console.WriteLine($"   Max Errors      {config.MaxErrors}");
            Console.WriteLine($"   Prometheus      {config.Prometheus}");
            Console.WriteLine($"   Random          {config.Random}");
            Console.WriteLine($"   Run Loop        {config.RunLoop}");
            Console.WriteLine($"   Sleep           {config.Sleep}");
            Console.WriteLine($"   Strict Json     {config.StrictJson}");

            if (!string.IsNullOrWhiteSpace(config.Tag))
            {
                Console.WriteLine($"   Tag             {config.Tag}");
            }

            Console.WriteLine($"   Timeout         {config.Timeout}");
            Console.WriteLine($"   Verbose         {config.Verbose}");
            Console.WriteLine($"   Verbose Errors  {config.VerboseErrors}");
            Console.WriteLine($"   WebV Prefix     {config.WebvPrefix}");
            Console.WriteLine($"   WebV Suffix     {config.WebvSuffix}");
            Console.WriteLine($"   XML Summary     {config.XmlSummary}");

            return 0;
        }
    }
}
