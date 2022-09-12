// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Reflection;
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
        /// <param name="cancelTestRunExecution">The cancellation tokenSource.</param>
        /// <param name="testRunId">The Test Run Id.</param>
        public TestRunExecutionHelper(ITestRunService testRunService, ILogger logger, CancellationTokenSource cancelTestRunExecution, string testRunId)
        {
            this.testRunService = testRunService;
            this.logger = logger;
            this.cancelTestRunExecution = cancelTestRunExecution;
            this.testRunId = testRunId;
        }

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
            try
            {
                if (!cancellationRequestReceived)
                {
                    // get current TestRun document
                    var testRun = await this.testRunService.Get(this.testRunId);

                    // Check if hardStop was requested.
                    if (testRun.HardStop && testRun.HardStopTime == null)
                    {
                        logger.LogInformation(new EventId((int)LogLevel.Information, nameof(HardStopCheck)), SystemConstants.LoggerMessageAttributeName, $"{SystemConstants.TestRunCancellationRequestReceivedMessage} {this.testRunId}");

                        //Initiate Cancellation.
                        this.cancelTestRunExecution.Cancel(false);

                        cancellationRequestReceived = true;
                    }
                }

                return true;
            }
            catch (CosmosException ce)
            {
                logger.LogError(new EventId((int)LogLevel.Error, $"{HardStopCheck}"), ce, SystemConstants.CosmosException);
                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(new EventId((int)LogLevel.Error, $"{HardStopCheck}"), ex, SystemConstants.Exception);
                return false;
            }
        }
    }
}
