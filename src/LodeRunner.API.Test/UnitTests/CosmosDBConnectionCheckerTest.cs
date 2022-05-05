// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using LodeRunner.Core.Extensions;
using LodeRunner.Data;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace LodeRunner.API.Test.UnitTests
{
    /// <summary>
    /// Test CosmosDBRepository readiness.
    /// </summary>
    public class CosmosDBConnectionCheckerTest
    {

        private readonly ITestOutputHelper output;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDBConnectionCheckerTest"/> class.
        /// </summary>
        /// <param name="output">The output.</param>
        public CosmosDBConnectionCheckerTest(ITestOutputHelper output)
        {
            this.output = output;
            this.logger = CreateLodeRunnerServiceLogger(new Config());

        }

        /// <summary>
        /// Test successful Get CosmosReady.
        /// </summary>
        /// <returns>The asynchronous unit test.</returns>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task CanValidateGetCosmosReady()
        {
            CancellationTokenSource cancellationTokenSource = new ();

            int intervalSeconds = 4;

            var cosmosDBConnectionChecker = new CosmosDBConnectionChecker(this.GetCosmosDBReady, this.logger, cancellationTokenSource, intervalSeconds, retryLimit: 3);

            bool cancellationRequested = await this.DelayTimeout(cancellationTokenSource, intervalSeconds).ConfigureAwait(false);

            Assert.False(cancellationRequested, "Request cancellation is not expected");
        }

        /// <summary>
        /// Test successful Get Cosmos Not Ready.
        /// </summary>
        /// <returns>The asynchronous unit test.</returns>
        [Fact]
        [Trait("Category", "Unit")]
        public async Task CanValidateGetCosmosNotReady()
        {
            CancellationTokenSource cancellationTokenSource = new ();

            int intervalSeconds = 4;

            var cosmosDBConnectionChecker = new CosmosDBConnectionChecker(this.GetCosmosNotReady, this.logger, cancellationTokenSource, intervalSeconds, retryLimit: 3);

            bool cancellationRequested = await this.DelayTimeout(cancellationTokenSource, intervalSeconds).ConfigureAwait(false);

            Assert.True(cancellationRequested, "Request cancellation expected.");
        }

        /// <summary>
        ///  Create LodeRunnerService Logger.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <returns>The logger.</returns>
        private static ILogger<CosmosDBRepository> CreateLodeRunnerServiceLogger(Config config)
        {
            string projectName = Assembly.GetCallingAssembly().GetName().Name;
            using var loggerFactory = LoggerFactory.Create(logger =>
            {
                logger.Setup(logLevelConfig: config, logValues: config, projectName: projectName);
            });

            return loggerFactory.CreateLogger<CosmosDBRepository>();
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
        /// Waits until maximum waiting time has reached.
        /// </summary>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <param name="intervalSeconds">The interval seconds.</param>
        /// <returns>Whether the cancellation was requested or not.</returns>
        private async Task<bool> DelayTimeout(CancellationTokenSource cancellationTokenSource, int intervalSeconds)
        {
            int maxWaitingTime = CalculateTimeout(intervalSeconds);
            await Task.Delay(maxWaitingTime * 1000).ConfigureAwait(false);

            bool cancellationRequested = cancellationTokenSource.IsCancellationRequested;
            this.output.WriteLine($"Total Wait time: {maxWaitingTime} secs.\tCancellation Requested:{cancellationRequested}\t");
            return cancellationRequested;
        }

        /// <summary>
        /// Gets the cosmos database ready.
        /// </summary>
        /// <returns>true.</returns>
        private bool GetCosmosDBReady()
        {
            return true;
        }

        /// <summary>
        /// Gets the cosmos not ready.
        /// </summary>
        /// <returns>false</returns>
        private bool GetCosmosNotReady()
        {
            return false;
        }
    }
}
