// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LodeRunner.Test.Common;
using Xunit;

namespace LodeRunner.Test.UnitTests
{
    /// <summary>
    /// Command Line Arguments Unit Test.
    /// </summary>
    public class CommandLineArgs
    {
        // Facts: are tests which are always true. They test invariant conditions.
        // Theories: are tests which are only true for a particular set of data.

        /// <summary>
        /// Test the succeeded case  of LodeRunner in Await mode.
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public void AwaitMode_Success()
        {
            using var secretsHelper = new SecretsHelper();

            secretsHelper.CreateValidSecrets();

            try
            {
                var validArgs = new string[] { "-s", "https://worka.aks-sb2.com", "-f", "memory-baseline1.json", "--delay-start", "-1", "--secrets-volume", "secrets" };

                // Assert.Equal(0, await App.Main(validArgs)); // this will start the app and try to actually run it
                RootCommand root = App.BuildRootCommand();

                Assert.True(root.Parse(validArgs).Errors.Count == 0, "AwaitMode Success");

                // Assert.True(root.Parse("-s", "--server").Errors.Count == 0, "Server(s) to test");
                // Assert.True(root.Parse("-f", "--files").Errors.Count == 0, "List of files to test");
                // Assert.True(root.Parse("--zone").Errors.Count == 0, "Zone for logging");
                // Assert.True(root.Parse("--region").Errors.Count == 0, "Region for logging");
                // Assert.True(root.Parse("-p", "--prometheus").Errors.Count == 0, "Send metrics to Prometheus");
                // Assert.True(root.Parse("--tag").Errors.Count == 0, "Tag for logging");
                // Assert.True(root.Parse("-l", "--sleep").Errors.Count == 0, "Sleep (ms) between each request");
                // Assert.True(root.Parse("-j", "--strict-json").Errors.Count == 0, "Use strict json when parsing");
                // Assert.True(root.Parse("-u", "--base-url").Errors.Count == 0, "Base url for files");
                // Assert.True(root.Parse("-v", "--verbose").Errors.Count == 0, "Display verbose results");
                // Assert.True(root.Parse("-r", "--run-loop").Errors.Count == 0, "Run test in an infinite loop");
                // Assert.True(root.Parse("--verbose-errors").Errors.Count == 0, "Log verbose error messages");
                // Assert.True(root.Parse("--random").Errors.Count == 0, "Run requests randomly (requires --run-loop)");
                // Assert.True(root.Parse("--duration").Errors.Count == 0, "Test duration (seconds)  (requires --run-loop)");
                // Assert.True(root.Parse("--summary-minutes").Errors.Count == 0, "Display summary results (minutes)  (requires --run-loop)");
                // Assert.True(root.Parse("-t", "--timeout").Errors.Count == 0, "Request timeout (seconds)");
                // Assert.True(root.Parse("--max-concurrent").Errors.Count == 0, "Max concurrent requests");
                // Assert.True(root.Parse("--max-errors").Errors.Count == 0, "Max validation errors");
                // Assert.True(root.Parse("--delay-start").Errors.Count == 0, "Delay test start (seconds)");
                // Assert.True(root.Parse("--client-refresh").Errors.Count == 0, "How often to refresh HTTP client (seconds)");
                // Assert.True(root.Parse("-d", "--dry-run").Errors.Count == 0, "Validates configuration");
                // Assert.True(root.Parse("--secrets-volume").Errors.Count == 0, "Secrets Volume Path");
            }
            finally
            {
                SecretsHelper.DeleteSecrets();
            }
        }

        /// <summary>
        /// Test the Failure case of LodeRunned in Await mode.
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public void AwaitMode_Failure()
        {
            using var secretsHelper = new SecretsHelper();

            secretsHelper.CreateEmptySecrets();

            try
            {
                // var validArgs = new string[] { "-s", "https://worka.aks-sb2.com", "-f", "memory-baseline1.json", "--delay-start", "-1", "--secrets-volume", "secrets" };

                // Assert.Equal(1, await App.Main(validArgs)); // this will start the app and try to actually run it
                RootCommand root = App.BuildRootCommand();

                var errors = root.Parse(new string[] { "-s", "https://worka.aks-sb2.com", "-f", "memory-baseline1.json", "--delay-start", "-1" }).Errors;

                Assert.True(errors.Count == 1 || errors.Any(m => m.Message.Contains(SystemConstants.CmdLineValidationDelayStartAndEmptySecretsMessage)), "AwaitMode - missing secrets-volume");

                errors = root.Parse(new string[] { "-s", "https://worka.aks-sb2.com", "-f", "memory-baseline1.json", "--secrets-volume", "secrets" }).Errors;

                Assert.True(errors.Count == 1 || errors.Any(m => m.Message.Contains(SystemConstants.CmdLineValidationSecretsAndInvalidDelayStartMessage)), "AwaitMode - missing delay-start");
            }
            finally
            {
                SecretsHelper.DeleteSecrets();
            }
        }
    }
}
