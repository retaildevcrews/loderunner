// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.Core.CommandLine;

namespace LodeRunner.Services.Extensions
{
    public static class ClientModeExtensions
    {
        /// <summary>
        /// Create and start a new LodeRunner instance in command mode.
        /// </summary>
        /// <param name="args">Command line args</param>
        /// <param name="testRunId">TestRun id</param>
        /// <param name="cancellationTokenSource">Cancellation token source</param>
        /// <returns>LodeRunnerService.</returns>
        public static async Task<int> CreateAndStartLodeRunnerCommandMode(string[] args, string testRunId, CancellationTokenSource cancellationTokenSource)
        {
            LodeRunner.Config lrConfig = new ();
            RootCommand rootClient = LRCommandLine.GetRootCommand(args);

            // Create lrConfig from arguments
            rootClient.Handler = CommandHandler.Create<LodeRunner.Config>(async (lrConfig) =>
            {
                lrConfig.IsClientMode = false;
                lrConfig.TestRunId = testRunId;
                using var l8rService = new LodeRunnerService(lrConfig, cancellationTokenSource);

                return await l8rService.StartService();
            });

            return await rootClient.InvokeAsync(args).ConfigureAwait(true);
        }
    }
}
