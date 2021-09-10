// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using Ngsa.Middleware.CommandLine;

namespace LodeRunner
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
            RootCommand root = new ()
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
            root.AddOption(new Option<int>(new string[] { "--delay-start" }, Parsers.ParseIntGENegOne, true, "Delay test start (seconds)"));
            root.AddOption(new Option<int>(new string[] { "--client-refresh" }, Parsers.ParseIntGTZero, true, "How often to refresh HTTP client (seconds)"));
            root.AddOption(new Option<bool>(new string[] { "-d", "--dry-run" }, "Validates configuration"));
            root.AddOption(new Option<string>(new string[] { "--secrets-volume" }, Parsers.ParseString, true, "Secrets Volume Path"));

            // validate dependencies
            root.AddValidator(ValidateDependencies);

            return root;
        }

        // handle --dry-run
        internal static int DoDryRun(Config config)
        {
            DisplayAsciiArt();

            // display the config
            Console.WriteLine("dry run");
            Console.WriteLine($"   Server          {string.Join(',', config.Server)}");
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
            Console.WriteLine($"   Client Refresh  {config.ClientRefresh}");
            Console.WriteLine($"   Max Concurrent  {config.MaxConcurrent}");
            Console.WriteLine($"   Max Errors      {config.MaxErrors}");
            Console.WriteLine($"   Random          {config.Random}");
            Console.WriteLine($"   Timeout         {config.Timeout}");
            Console.WriteLine($"   Verbose         {config.Verbose}");
            Console.WriteLine($"   Secrets Volume  {config.SecretsVolume}");

            return SystemConstants.ExitSuccess;
        }

        /// <summary>Validates combinations of parameters and dependencies.</summary>
        /// <param name="result">The ComandResult object</param>
        /// <returns> empty string if no issues found </returns>
        private static string ValidateDependencies(CommandResult result)
        {
            string msg = string.Empty;

            OptionResult runLoopRes = result.Children.FirstOrDefault(c => c.Symbol.Name == "run-loop") as OptionResult;
            OptionResult durationRes = result.Children.FirstOrDefault(c => c.Symbol.Name == "duration") as OptionResult;
            OptionResult randomRes = result.Children.FirstOrDefault(c => c.Symbol.Name == "random") as OptionResult;
            OptionResult secretsRes = result.Children.FirstOrDefault(c => c.Symbol.Name == "secrets-volume") as OptionResult;
            OptionResult delayStartRes = result.Children.FirstOrDefault(c => c.Symbol.Name == "delay-start") as OptionResult;

            bool runLoop = runLoopRes.GetValueOrDefault<bool>();
            int? duration = durationRes.GetValueOrDefault<int?>();
            bool random = randomRes.GetValueOrDefault<bool>();
            string secrets = secretsRes?.GetValueOrDefault<string>();
            int? delayStart = delayStartRes.GetValueOrDefault<int?>();

            if (duration != null && duration > 0 && !runLoop)
            {
                msg += "--run-loop must be true to use --duration\n";
            }
            else if (random && !runLoop)
            {
                msg += "--run-loop must be true to use --random\n";
            }

            // validate secrets volume on delay start
            if (delayStart == -1 && string.IsNullOrWhiteSpace(secrets))
            {
                msg += "--secrets-volume cannot be empty when --delay-start is equals to -1\n";
            }
            else if (!string.IsNullOrWhiteSpace(secrets) && delayStart != -1)
            {
                msg += $"--secrets-volume requires --delay-start to be equals to negative one (-1)\n";
            }
            else if (delayStart == -1 && !Directory.Exists(secrets))
            {
                msg += $"--secrets-volume ({secrets}) does not exist\n";
            }

            return msg;
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
