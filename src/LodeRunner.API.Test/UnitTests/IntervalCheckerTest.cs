// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.Core;
using LodeRunner.Core.Extensions;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace LodeRunner.API.Test.UnitTests
{
    /// <summary>
    /// Test Interval Checker class.
    /// </summary>
    public class IntervalCheckerTest
    {
        private const int IntervalSeconds = 4;
        private const int RetryLimit = 3;

        private readonly ITestOutputHelper output;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntervalCheckerTest"/> class.
        /// </summary>
        /// <param name="output">The output.</param>
        public IntervalCheckerTest(ITestOutputHelper output)
        {
            this.output = output;
            this.logger = CreateLogger(new Config() { LogLevel = LogLevel.Warning });
        }

        /// <summary>
        /// Test for cancellation request was initiated have reached the timeout based on Interval and RetryLimit parameters.
        /// </summary>
        /// <returns>The asynchronous unit test.</returns>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task CanValidateCancellationRequestNotInitiated()
        {
            bool cancellationRequested = await this.RunIntervalCheckerWaitAndGetCancellationStatus(ReturnScalarValueAsFunctionTask(true), IntervalSeconds, RetryLimit);

            // Validate Cancellation Request
            Assert.False(cancellationRequested, "Request cancellation is not expected.");
        }

        /// <summary>
        /// Test for cancellation request was not initiated after have reached the timeout based on Interval and RetryLimit parameters.
        /// </summary>
        /// <returns>The asynchronous unit test.</returns>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task CanValidateCancellationRequestInitiated()
        {
            bool cancellationRequested = await this.RunIntervalCheckerWaitAndGetCancellationStatus(ReturnScalarValueAsFunctionTask(false), IntervalSeconds, RetryLimit);

            // Validate Cancellation Request
            Assert.True(cancellationRequested, "Request cancellation expected.");
        }

        /// <summary>
        ///  Create IntervalCheckerTest Logger.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <returns>The logger.</returns>
        private static ILogger<IntervalCheckerTest> CreateLogger(Config config)
        {
            string projectName = Assembly.GetCallingAssembly().GetName().Name;
            using var loggerFactory = LoggerFactory.Create(logger =>
            {
                logger.Setup(logLevelConfig: config, logValues: config, projectName: projectName);
            });

            return loggerFactory.CreateLogger<IntervalCheckerTest>();
        }

        /// <summary>
        /// Calculate the maximum waiting time.
        /// </summary>
        /// <param name="interval">The interval.</param>
        /// <returns>the max waiting time.</returns>
        private static int CalculateTimeout(int interval)
        {
            int newInterval = interval;

            for (int i = 1; i < 3; i++)
            {
                newInterval /= 2;
                interval += newInterval;
            }

            // We will wait for Max Interval time plus 1 extra second.
            return interval + 1;
        }

        /// <summary>
        /// Returns the scalar value as Function Task.
        /// The IntervalChecker constructor requires a function pointer to a task with a boolean response, to perform the boolean Check.
        /// </summary>
        /// <typeparam name="T">The type of the scalar value.</typeparam>
        /// <param name="response">The response to be wrapped in the function Task.</param>
        /// <returns>The Task.</returns>
        private static Func<Task<T>> ReturnScalarValueAsFunctionTask<T>(T response)
        {
            return new (() => Task.Run(() => { return response; }));
        }

        /// <summary>
        /// Runs the interval checker and waits until timeout and get cancellation token status and console output.
        /// </summary>
        /// <param name="taskToExecute">The task to execute.</param>
        /// <param name="intervalSeconds">The interval seconds.</param>
        /// <param name="retryLimit">The retry limit.</param>
        /// <returns>The cancellationToken status and the outputAsStringList.</returns>
        private async Task<bool> RunIntervalCheckerWaitAndGetCancellationStatus(Func<Task<bool>> taskToExecute, int intervalSeconds = 4, int retryLimit = 3)
        {
            CancellationTokenSource cancellationTokenSource = new ();

            using var intervalChecker = new IntervalChecker(taskToExecute, this.logger, cancellationTokenSource, intervalSeconds, retryLimit);

            intervalChecker.Start();

            return await this.WaitForTimeoutAndGetCancellationRequestStatus(cancellationTokenSource, intervalSeconds);
        }

        /// <summary>
        /// Waits until timeout is reached and determines if Cancellation request is in place.
        /// </summary>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <param name="intervalSeconds">The interval seconds.</param>
        /// <returns>Whether the cancellation was requested or not.</returns>
        private async Task<bool> WaitForTimeoutAndGetCancellationRequestStatus(CancellationTokenSource cancellationTokenSource, int intervalSeconds)
        {
            int maxWaitingTime = CalculateTimeout(intervalSeconds);
            await Task.Delay(maxWaitingTime * 1000).ConfigureAwait(false);

            bool cancellationRequested = cancellationTokenSource.IsCancellationRequested;
            this.output.WriteLine($"Total Wait time: {maxWaitingTime} secs.\tCancellation Requested: {cancellationRequested}\t");
            return cancellationRequested;
        }
    }
}
