// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Threading.Tasks;
using LodeRunner.Core.CommandLine;
using Xunit;

namespace LodeRunner.Test.UnitTests
{
    /// <summary>
    /// ConfigServer Names Unit Tests.
    /// </summary>
    public class ConfigServerNamesTest
    {
        /// <summary>
        /// LoderRunner Command mode, validate configuration set default values.
        /// </summary>
        /// <param name="serverNames">The serverNames arguments.</param>
        /// <param name="messageIfFailed">The message if failed.</param>
        [Theory]
        [Trait("Category", "Unit")]
        [InlineData(new string[] { "ngsa-memory-eastus-pre.cse.ms", "ngsa-memory-eastus-pre.cse.ms/" }, "Unable to match Server name without prefix.")]
        [InlineData(new string[] { "http://ngsa-memory-eastus-pre.cse.ms", "http://ngsa-memory-eastus-pre.cse.ms/" }, "Unable to match Server name with 'http' prefix.")]
        [InlineData(new string[] { "https://ngsa-memory-eastus-pre.cse.ms", "https://ngsa-memory-eastus-pre.cse.ms/" }, "Unable to match Server name with 'https' prefix.")]
        [InlineData(new string[] { "localhost", }, "Unable to match Server name 'localhost'")]
        public void CommandMode_ValidateLodeRunnerConfigServerDefaultValues(string[] serverNames, string messageIfFailed)
        {
            Config lrConfig = new ()
            {
                Server = serverNames.ToList(),
            };

            List<string> originalServerNames = new ();

            originalServerNames.AddRange(lrConfig.Server);

            lrConfig.SetDefaultValues();

            foreach (var serverName in originalServerNames)
            {
                bool found;
                if (!serverName.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    if (serverName.StartsWith("localhost", StringComparison.OrdinalIgnoreCase) || serverName.StartsWith("127.0.0.1", StringComparison.OrdinalIgnoreCase))
                    {
                        found = lrConfig.Server.Contains($"http://{serverName}");
                    }
                    else
                    {
                        found = lrConfig.Server.Contains($"https://{serverName}");
                    }
                }
                else
                {
                    found = lrConfig.Server.Contains(serverName);
                }

                Assert.True(found, messageIfFailed);
            }
        }
    }
}
