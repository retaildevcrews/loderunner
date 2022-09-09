// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.Core;
using LodeRunner.Data.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace LodeRunner
{
    /// <summary>
    /// Represents the Test RunExecution Helper class.
    /// </summary>
    internal class TestRunExecutionHelper : IDisposable
    {
        private readonly ILogger logger;
        private readonly ITestRunService testRunService;
        private readonly string testRunId;
        private readonly CancellationTokenSource cancelTestRunExecution;
        private bool cancellationRequestReceived = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestRunExecutionHelper"/> class.
        /// TestRunExecutionChecker Constructor
        /// </summary>
        /// <param name="testRunService">The TestRun Service.</param>
        /// <param name="logger">The Logger.</param>
        /// <param name="cancelTestRunExecution">The cancelation tokenSource.</param>
        /// <param name="testRunId">The Test Run Id.</param>
        public TestRunExecutionHelper(ITestRunService testRunService, ILogger logger, CancellationTokenSource cancelTestRunExecution, string testRunId)
        {
            this.testRunService = testRunService;
            this.logger = logger;
            this.cancelTestRunExecution = cancelTestRunExecution;
            this.testRunId = testRunId;
        }

        /// <summary>
        /// Reusable try-catch block to encapsulate taskExecution while handling plain cosmos exceptions and other exceptions.
        /// </summary>
        /// <param name="logger">The ILogger.</param>
        /// <param name="methodName">String containing caller member name to improve logging.</param>
        /// <param name="taskToExecute">Task to be executed in try block</param>
        /// <returns>A task indicating whether or not an exception was thrown.</returns>
        public static async Task<bool> TryCatchException(ILogger logger, string methodName, Func<Task<bool>> taskToExecute)
        {
            try
            {
                return await taskToExecute();
            }
            catch (CosmosException ce)
            {
                logger.LogError(new EventId((int)LogLevel.Error, $"{methodName}"), ce, SystemConstants.CosmosException);
                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(new EventId((int)LogLevel.Error, $"{methodName}"), ex, SystemConstants.Exception);
                return false;
            }
        }

        ///// <summary>
        ///// Reusable try-catch block to encapsulate PreconditionFailed Exception for taskExecution.
        ///// </summary>
        ///// <param name="logger">The ILogger.</param>
        ///// <param name="methodName">String containing caller member name to improve logging.</param>
        ///// <param name="taskToExecute">Task to be executed in try block</param>
        ///// <returns>A task indicating whether or not an exception was thrown.</returns>
        //public static async Task<bool> TryCatchPreconditionFailedException(ILogger logger, string methodName, Func<Task<bool>> taskToExecute)
        //{
        //    try
        //    {
        //        return await taskToExecute();
        //    }
        //    catch (CosmosException ce)
        //    {
        //        if (ce.StatusCode == System.Net.HttpStatusCode.PreconditionFailed)
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            logger.LogError(new EventId((int)LogLevel.Error, $"{methodName}"), ce, SystemConstants.CosmosException);
        //            return false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(new EventId((int)LogLevel.Error, $"{methodName}"), ex, SystemConstants.Exception);
        //        return false;
        //    }
        //}

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Determines whether the TestRun has Hard Stop.
        /// </summary>
        /// <returns>
        /// True indicates that the Check step complete without exceptions.
        /// </returns>
        public async Task<bool> HardStopCheck()
        {
            return await TryCatchException(logger, nameof(HardStopCheck), async () =>
            {
                // get current TestRun document
                var testRun = await this.testRunService.Get(this.testRunId);

                // Check if hardStop was requested.
                if (testRun.HardStop && testRun.HardStopTime == null && !cancellationRequestReceived)
                {
                    logger.LogInformation(new EventId((int)LogLevel.Information, nameof(HardStopCheck)), SystemConstants.LoggerMessageAttributeName, $"{SystemConstants.TestRunCancellationRequestReceivedMessage} {this.testRunId}");

                    //Initiate Cancellation.
                    this.cancelTestRunExecution.Cancel(false);

                    cancellationRequestReceived = true;
                }

                return true;
            });
        }
    }
}
