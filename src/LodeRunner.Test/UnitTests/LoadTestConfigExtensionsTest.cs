// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using LodeRunner.Core.CommandLine;
using LodeRunner.Core.Extensions;
using LodeRunner.Core.Models;
using Xunit;

namespace LodeRunner.Test.UnitTests
{
    /// <summary>
    /// LoadTestConfigExtensions Unit Tests.
    /// </summary>
    public class LoadTestConfigExtensionsTest
    {
        /// <summary>
        /// Creates MemberData parameter values for GetArgs test cases.
        /// </summary>
        /// <returns>Parameter values.</returns>
        public static IEnumerable<object[]> GetArgsLoadTestConfigParams()
        {
            yield return new object[] { new LoadTestConfig() { Server = new List<string> { "https://somerandomdomain.com" }, Files = new List<string> { "memory-baseline.json" }, Sleep = 1000 }, 6, "Validation for run once" };
            yield return new object[] { new LoadTestConfig() { Server = new List<string> { "https://somerandomdomain.com" }, Files = new List<string> { "memory-baseline.json" }, Duration = 1, RunLoop = true }, 8, "Validation for run loop" };
            yield return new object[] { new LoadTestConfig() { Server = new List<string> { "https://somerandomdomain.com", "https://someotherdomain.com" }, Files = new List<string> { "memory-baseline.json", "memory-benchmark.json" }, Duration = 1, RunLoop = true }, 10, "Validation for multiple servers and files listed" };
        }

        /// <summary>
        /// Test GetArgs Extension method to ensure conversion works as expected.
        /// </summary>
        /// <param name="loadTestConfig">The LoadTestConfig to convert to command line arguments.</param>
        /// <param name="expectedArgsLength">The expected length of the generated args array.</param>
        /// <param name="messageifFailed">The message to display if test failed.</param>
        [Theory]
        [Trait("Category", "Unit")]
        [MemberData(nameof(GetArgsLoadTestConfigParams))]
        public void GetArgs(LoadTestConfig loadTestConfig, int expectedArgsLength, string messageifFailed)
        {
            string[] args = LoadTestConfigExtensions.GetArgs(loadTestConfig);

            // assert that no unexpected args are present or missing
            Assert.Equal(expectedArgsLength, args.Length);

            RootCommand root = LRCommandLine.GetRootCommand(args);

            // assert that the args can be parsed successfully
            Assert.True(root.Parse(args).Errors.Count == 0, messageifFailed);
        }
    }
}
