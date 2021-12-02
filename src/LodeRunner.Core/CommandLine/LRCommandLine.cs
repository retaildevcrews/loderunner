// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using LodeRunner.Core.Interfaces;

namespace LodeRunner.Core.CommandLine
{
    /// <summary>
    /// Main application class.
    /// </summary>
    public sealed class LRCommandLine
    {
        /// <summary>
        /// Gets the correct root command by parsing --mode.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>RootCommand based on --mode.</returns>
        public static RootCommand GetRootCommand(string[] args)
        {
            RootCommand rootCommand;

            if (IsClientMode(args))
            {
                rootCommand = BuildRootClientMode();
            }
            else
            {
                rootCommand = BuildRootCommandMode();
            }

            return rootCommand;
        }

        /// <summary>
        /// Build the RootCommandLine Mode for parsing.
        /// </summary>
        /// <returns>RootCommand.</returns>
        public static RootCommand BuildRootCommandMode()
        {
            RootCommand root = new ()
            {
                Name = SystemConstants.LodeRunnerCommandMode,
                Description = "Validate API responses",
                TreatUnmatchedTokensAsErrors = true,
            };

            // Note: --mode parameter is validated at Program.cs, we only needed here so RootCommand will complain about unrecognized arguments
            root.AddOption(new Option<string>("--mode", description: "Startup mode [Command|Client], if missing, defaults to Command"));
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
            root.AddOption(new Option<int>(new string[] { "--delay-start" }, Parsers.ParseIntGEZero, true, "Delay test start (seconds)"));
            root.AddOption(new Option<bool>(new string[] { "-d", "--dry-run" }, "Validates configuration"));
            root.AddOption(new Option<string>(new string[] { "--secrets-volume" }, Parsers.ParseString, true, "Secrets Volume Path"));

            // validate dependencies
            root.AddValidator(ValidateCommandModeDependencies);

            return root;
        }

        /// <summary>
        /// Build the RootClient Mode for parsing.
        /// </summary>
        /// <returns>RootCommand.</returns>
        public static RootCommand BuildRootClientMode()
        {
            RootCommand root = new ()
            {
                Name = SystemConstants.LodeRunnerClientMode,
                Description = "Waits for a job",
                TreatUnmatchedTokensAsErrors = true,
            };

            // Note: --mode parameter is validated at Program.cs, we only needed here so RootCommand will complain about unrecognized arguments
            root.AddOption(new Option<string>("--mode", description: "Startup mode [Command|Client], id missing defaults to Command"));
            root.AddOption(new Option<string>(new string[] { "--zone" }, Parsers.ParseString, true, "Zone for logging"));
            root.AddOption(new Option<string>(new string[] { "--region" }, Parsers.ParseString, true, "Region for logging"));
            root.AddOption(new Option<bool>(new string[] { "-p", "--prometheus" }, Parsers.ParseBool, true, "Send metrics to Prometheus"));
            root.AddOption(new Option<string>(new string[] { "--tag" }, Parsers.ParseString, true, "Tag for logging"));
            root.AddOption(new Option<bool>(new string[] { "-d", "--dry-run" }, "Validates configuration"));
            root.AddOption(new Option<string>(new string[] { "--secrets-volume" }, Parsers.ParseString, true, "Secrets Volume Path"));

            // validate dependencies
            root.AddValidator(ValidateClientModeDependencies);

            return root;
        }

        /// <summary>
        /// Does the dry run.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns>Success or Failure code.</returns>
        public static int DoDryRun(ILRConfig config)
        {
            if (config.IsClientMode)
            {
                return DoDryRunClientMode(config);
            }
            else
            {
                return DoDryRunCommandMode(config);
            }
        }

        /// <summary>
        /// handle --dry-run. for Command Mode.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns>Success or Failure code.</returns>
        private static int DoDryRunCommandMode(ILRConfig config) // this needs to be interface.
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

            return SystemConstants.ExitSuccess;
        }

        /// <summary>
        /// handle --dry-run for Client Mode.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns>Success or Failure code.</returns>
        private static int DoDryRunClientMode(ILRConfig config) // this needs to be interface.
        {
            DisplayAsciiArt();

            // display the config
            Console.WriteLine("dry run");
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

            Console.WriteLine($"   Prometheus      {config.Prometheus}");
            Console.WriteLine($"   Secrets Volume  {config.SecretsVolume}");

            return SystemConstants.ExitSuccess;
        }

        /// <summary>Validates combinations of parameters and dependencies.</summary>
        /// <param name="result">The ComandResult object.</param>
        /// <returns> empty string if no issues found.</returns>
        private static string ValidateCommandModeDependencies(CommandResult result)
        {
            string msg = string.Empty;

            OptionResult runLoopRes = result.Children.FirstOrDefault(c => c.Symbol.Name == "run-loop") as OptionResult;
            OptionResult durationRes = result.Children.FirstOrDefault(c => c.Symbol.Name == "duration") as OptionResult;
            OptionResult randomRes = result.Children.FirstOrDefault(c => c.Symbol.Name == "random") as OptionResult;

            bool runLoop = runLoopRes.GetValueOrDefault<bool>();
            int? duration = durationRes.GetValueOrDefault<int?>();
            bool random = randomRes.GetValueOrDefault<bool>();

            if (duration != null && duration > 0 && !runLoop)
            {
                msg += $"{SystemConstants.CmdLineValidationDurationAndLoopMessage}\n";
            }
            else if (random && !runLoop)
            {
                msg += $"{SystemConstants.CmdLineValidationRandomAndLoopMessage}\n";
            }

            return msg;
        }

        /// <summary>Validates combinations of parameters and dependencies.</summary>
        /// <param name="result">The ComandResult object.</param>
        /// <returns> empty string if no issues found.</returns>
        private static string ValidateClientModeDependencies(CommandResult result)
        {
            string msg = string.Empty;

            OptionResult secretsRes = result.Children.FirstOrDefault(c => c.Symbol.Name == "secrets-volume") as OptionResult;

            string secrets = secretsRes?.GetValueOrDefault<string>();

            // validate secrets volume on delay start
            if (string.IsNullOrWhiteSpace(secrets))
            {
                msg += $"{SystemConstants.CmdLineValidationClientModeAndEmptySecretsMessage}\n";
            }
            else if (!Directory.Exists(secrets))
            {
                msg += $"{SystemConstants.CmdLineValidationSecretsVolumeBeginningMessage}{secrets}{SystemConstants.CmdLineValidationSecretsVolumeEndMessage}\n";
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

        /// <summary>
        /// Determines whether [is client mode] [the specified arguments].
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>
        ///   <c>true</c> if [is client mode] [the specified arguments]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsClientMode(string[] args)
        {
            var argsAsList = args.ToList();
            int modeIndex = argsAsList.IndexOf("--mode");
            if (modeIndex < 0)
            {
                return false;
            }
            else
            {
                string mode = argsAsList[modeIndex + 1];
                if (string.IsNullOrWhiteSpace(mode))
                {
                    return false;
                }

                return mode == "Client";
            }
        }
    }
}
