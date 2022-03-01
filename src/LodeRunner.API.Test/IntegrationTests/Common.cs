// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace LodeRunner.API.Test.IntegrationTests
{
    /// <summary>
    /// REpresent common use functions.
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// Runs and retry.
        /// </summary>
        /// <param name="maxRetries">The maximum retries.</param>
        /// <param name="maxDelay">The maximum delay.</param>
        /// <param name="taskSource">Task cancelationSource.</param>
        /// <param name="taskToExecute">The task to execute.</param>
        /// <returns>the task.</returns>
        public static async Task RunAndRetry(int maxRetries, int maxDelay, CancellationTokenSource taskSource, Func<int, Task> taskToExecute)
        {
            for (int i = 1; i <= maxRetries; i++)
            {
                await taskToExecute(i);

                if (taskSource.IsCancellationRequested)
                {
                    break;
                }

                await Task.Delay(maxDelay).ConfigureAwait(false);
            }
        }
    }
}
