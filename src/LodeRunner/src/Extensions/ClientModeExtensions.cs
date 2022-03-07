// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.Core.CommandLine;
using Microsoft.Extensions.Logging;

namespace LodeRunner.Services.Extensions
{
    public static class ClientModeExtensions
    {
        /// <summary>
        /// Create and start a new LodeRunner instance in command mode.
        /// </summary>
        /// <param name="args">Command line args</param>
        /// <param name="clientStatusId">ClientStatus id.</param>
        /// <param name="loadClientId">LoadClient id.</param>
        /// <param name="testRunId">TestRun id.</param>
        /// <param name="cancellationTokenSource">Cancellation token source.</param>
        /// <param name="logger">The logger.</param>
        /// <returns>LodeRunnerService.</returns>
        public static async Task<int> CreateAndStartLodeRunnerCommandMode(string[] args, string clientStatusId, string loadClientId, string testRunId, CancellationTokenSource cancellationTokenSource, ILogger<LodeRunnerService> logger)
        {
            LodeRunner.Config lrConfig = new ();
            RootCommand rootClient = LRCommandLine.GetRootCommand(args);

            if (string.IsNullOrEmpty(clientStatusId))
            {
                throw new Exception("clientStatusId is null or empty.");
            }

            if (string.IsNullOrEmpty(testRunId))
            {
                throw new Exception("testRunId is null or empty");
            }

            if (string.IsNullOrEmpty(loadClientId))
            {
                throw new Exception("loadClientId is null or empty");
            }

            // Create lrConfig from arguments
            rootClient.Handler = CommandHandler.Create<LodeRunner.Config>(async (lrConfig) =>
            {
                lrConfig.IsClientMode = false;
                lrConfig.TestRunId = testRunId;
                lrConfig.ClientStatusId = clientStatusId;
                lrConfig.LoadClientId = loadClientId;
                using var l8rService = new LodeRunnerService(lrConfig, cancellationTokenSource, logger, useIdValuesFromConfig: true);

                return await l8rService.StartService();
            });

            return await rootClient.InvokeAsync(args).ConfigureAwait(true);
        }
    }
}
