// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using LodeRunner.Core.Interfaces;

namespace LodeRunner.Core
{
    /// <summary>
    /// Represent common use functions.
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// Sets the run task clear ngsa logger identifier values.
        /// </summary>
        /// <param name="config">The LR config.</param>
        /// <param name="taskToExecute">The task to execute.</param>
        /// <returns> The task.</returns>
        public static async Task SetRunTaskClearNgsaLoggerIdValues(ILRConfig config, Func<Task> taskToExecute)
        {
            try
            {
                NgsaLogger.NgsaLogger.ClientStatusId = config.ClientStatusId;
                NgsaLogger.NgsaLogger.TestRunId = config.TestRunId;
                NgsaLogger.NgsaLogger.LoadClientId = config.LoadClientId;

                await taskToExecute();
            }
            finally
            {
                NgsaLogger.NgsaLogger.LoadClientId = string.Empty;
                NgsaLogger.NgsaLogger.ClientStatusId = string.Empty;
                NgsaLogger.NgsaLogger.TestRunId = string.Empty;
            }
        }
    }
}
