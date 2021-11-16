// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LodeRunner.Core.CommandLine;
using LodeRunner.Test.Common;
using Xunit;

namespace LodeRunner.Test.UnitTests
{
    /// <summary>
    /// Command Line Arguments Unit Test.
    /// </summary>
    public class CommandLineArgs
    {
        /// <summary>
        /// Test the success case of LodeRunner in Await mode.
        /// </summary>
        [Fact]
        [Trait("Category", "Unit")]
        public void AwaitMode_Success()
        {
            using var secretsHelper = new SecretsHelper();

            secretsHelper.CreateValidSecrets();
            try
            {
                var validArgs = new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--delay-start", "-1", "--secrets-volume", "secrets" };

                RootCommand root = LRCommandLine.BuildRootCommand();

                Assert.True(root.Parse(validArgs).Errors.Count == 0, "All arguments supplied.");
            }
            finally
            {
                SecretsHelper.DeleteSecrets();
            }
        }

        /// <summary>
        /// Test the failure case of LodeRunner in Await mode.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="expectedErrorMessage">The expected error message.</param>
        /// <param name="messageifFailed">The message to display if test failed.</param>
        [Theory]
        [Trait("Category", "Unit")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--delay-start", "-1" }, Core.SystemConstants.CmdLineValidationDelayStartAndEmptySecretsMessage, "secrets-volume is not supplied")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--secrets-volume", "secrets" }, Core.SystemConstants.CmdLineValidationSecretsAndInvalidDelayStartMessage, "delay-start is not supplied")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--delay-start", "-1", "--secrets-volume", "secrets" }, Core.SystemConstants.CmdLineValidationSecretsVolumeEndMessage, "secrets folder does not exist")]
        public void AwaitMode_Failure(string[] args, string expectedErrorMessage, string messageifFailed)
        {
            using var secretsHelper = new SecretsHelper();

            SecretsHelper.DeleteSecrets();

            RootCommand root = LRCommandLine.BuildRootCommand();

            var errors = root.Parse(args).Errors;

            Assert.True(errors.Count == 1 && errors.Any(m => m.Message.Contains(expectedErrorMessage)), messageifFailed);
        }

        /// <summary>
        /// Test the success case of LodeRunner in Traditional mode.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="messageifFailed">The message to display if test failed.</param>
        [Theory]
        [Trait("Category", "Unit")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--duration", "1", "--run-loop", "true" }, "Validation for --duration.")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--random", "true", "--run-loop", "true" }, "Validation for --random")]
        public void TraditionalMode_ValidateDependencies_Success(string[] args, string messageifFailed)
        {
            RootCommand root = LRCommandLine.BuildRootCommand();

            var errors = root.Parse(args).Errors;

            Assert.True(errors.Count == 0, messageifFailed);
        }

        /// <summary>
        /// Test the failure case of LodeRunner in Traditional mode.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="expectedErrorMessage">The expected error message.</param>
        /// <param name="messageifFailed">The message to display if test failed.</param>
        [Theory]
        [Trait("Category", "Unit")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--duration", "1" }, Core.SystemConstants.CmdLineValidationDurationAndLoopMessage, "Validation for --duration.")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--random", "true" }, Core.SystemConstants.CmdLineValidationRandomAndLoopMessage, "Validation for --random")]
        public void TraditionalMode_ValidateDependencies_Failure(string[] args, string expectedErrorMessage, string messageifFailed)
        {
            RootCommand root = LRCommandLine.BuildRootCommand();

            var errors = root.Parse(args).Errors;

            Assert.True(errors.Count == 1 && errors.Any(m => m.Message.Contains(expectedErrorMessage)), messageifFailed);
        }

        /// <summary>
        /// Test the failure case of LodeRunner in Traditional mode when validating dependencies for the ones that use GetValueOrDefault.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="expectedErrorMessage">The expected error message.</param>
        /// <param name="messageifFailed">The message to display if test failed.</param>
        [Theory]
        [Trait("Category", "Unit")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--duration", "-2" }, "must be an integer >= 1", "Argument --duration")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--secrets-volume", "go" }, "must be at least 3 characters", "Argument --secrets-volume")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--delay-start", "-2" }, "must be an integer >= -1", "Argument --delay-start")]
        public void TraditionalMode_ValidateDependencies_With_Invalid_Data_And_GetValueOrDefault(string[] args, string expectedErrorMessage, string messageifFailed)
        {
            // Note: we do not need to test for "--run-loop" and "--random" since they are both boolean type and default to "true", so exception is never thrown.
            RootCommand root = LRCommandLine.BuildRootCommand();

            try
            {
                var errors = root.Parse(args).Errors;
                Assert.True(false, "Previous line should have thrown a General Exception based on InLineData. InlineData is not appropriate for this Unit Test.");
            }
            catch (Xunit.Sdk.TrueException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Assert.True(ex.Message.Contains(expectedErrorMessage), messageifFailed);
            }
        }

        /// <summary>
        /// Test the success cases of LodeRunner in Traditional mode.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="messageifFailed">The message to display if test failed.</param>
        [Theory]
        [Trait("Category", "Unit")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json" }, "Minimum requirements met")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--delay-start", "5" }, "Requirements met for argument --delay-start")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--max-errors", "1" }, "Requirements met for argument --max-errors")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--zone", "central" }, "Requirements met for argument --zone")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--region", "US-central" }, "Requirements met for argument --region")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--prometheus", "true" }, "Requirements met for argument --prometheus")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--tag", "myTag" }, "Requirements met for argument --tag")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--sleep", "500" }, "Requirements met for argument --tag")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--strict-json", "false" }, "Requirements met for argument --strict-json")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--base-url", "https://" }, "Requirements met for argument --base-url")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--verbose", "true" }, "Requirements met for argument --verbose")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--run-loop", "true" }, "Requirements met for argument --run-loop")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--verbose-errors", "true" }, "Requirements met for argument --verbose-errors")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--summary-minutes", "1" }, "Requirements met for argument --summary-minutes")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--timeout", "10" }, "Requirements met for argument --timeout")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--max-concurrent", "10" }, "Requirements met for argument --max-concurrent")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--client-refresh", "10" }, "Requirements met for argument --client-refresh")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--dry-run", "true" }, "Requirements met for argument --dry-run")]
        public void TraditionalMode_Success(string[] args, string messageifFailed)
        {
            // Note: "--random", "--duration", "--secrets-volume" and "--delay-start" are tested on different Unit Tests.
            RootCommand root = LRCommandLine.BuildRootCommand();

            var errors = root.Parse(args).Errors;

            Assert.True(errors.Count == 0, messageifFailed);
        }

        /// <summary>
        /// Test the failure cases of LodeRunner in Traditional mode.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="expectedErrorMessage">The expected error message.</param>
        /// <param name="messageifFailed">The message to display if test failed.</param>
        [Theory]
        [Trait("Category", "Unit")]
        [InlineData(new string[] { "-s", "-f", "memory-baseline.json", }, "Required argument missing for option: -s", "Minimum requirements not met for argument -- server")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f" }, "Required argument missing for option: -f", "Minimum requirements not met for argument --files")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--max-errors", "-1" }, "must be an integer >= 1", "Argument --max-errors")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--zone", "5" }, "must be at least 3 characters", "Minimum requirements not met for argument --zone")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--region", "US" }, "must be at least 3 characters", "Minimum requirements not met for argument --region")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--prometheus", "2" }, "Unrecognized command or argument", "Minimum requirements not met for argument --prometheus")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--tag", "tg" }, "must be at least 3 characters", "Minimum requirements not met for argument --tag")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--sleep", "sleep" }, "must be an integer >= 1", "Minimum requirements not met for argument --sleep")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--strict-json", "1" }, "Unrecognized command or argument", "Minimum requirements not met for argument --strict-json")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--base-url", "ur" }, "must be at least 3 characters", "Minimum requirements not met for argument --base-url")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--verbose", "0" }, "Unrecognized command or argument", "Minimum requirements not met for argument --verbose")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--run-loop", "0" }, "Unrecognized command or argument", "Minimum requirements not met for argument --run-loop")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--verbose-errors", "0" }, "Unrecognized command or argument", "Minimum requirements not met for argument --verbose-errors")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--summary-minutes", "time" }, "must be an integer >= 1", "Minimum requirements not met for argument --summary-minutes")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--timeout", "-1" }, "must be an integer >= 1", "Minimum requirements not met for argument --timeout")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--max-concurrent", "-1" }, "must be an integer >= 1", "Minimum requirements not met for argument --max-concurrent")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--client-refresh", "-1" }, "must be an integer >= 1", "Minimum requirements not met for argument --client-refresh")]
        [InlineData(new string[] { "-s", "https://somerandomdomain.com", "-f", "memory-baseline.json", "--dry-run", "0" }, "Unrecognized command or argument", "Minimum requirements not met for argument --dry-run")]
        public void TraditionalMode_Failure(string[] args, string expectedErrorMessage, string messageifFailed)
        {
            // Note: "--random", "--duration", "--secrets-volume" and "--delay-start" are tested on different  Unit Tests.
            RootCommand root = LRCommandLine.BuildRootCommand();

            var errors = root.Parse(args).Errors;
            Assert.True(errors.Count == 1 && errors.Any(m => m.Message.Contains(expectedErrorMessage)), messageifFailed);
        }
    }
}
