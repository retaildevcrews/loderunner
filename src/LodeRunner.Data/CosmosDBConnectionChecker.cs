// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace LodeRunner.Data
{
    /// <summary>
    /// Helper class to check Cosmos DB Connection.
    /// </summary>
    public class CosmosDBConnectionChecker
    {
        private readonly ILogger logger;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly int retryLimit = 3;

        private readonly Func<bool> getIsCosmosDBReady;
        private int failuresCount = 1;
        private System.Timers.Timer dbConnectionHealthCheck = default;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDBConnectionChecker"/> class.
        /// </summary>
        /// <param name="getIsCosmosDBReady">The get is cosmos database ready.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <param name="interval">The interval (secs).</param>
        /// <param name="retryLimit">The retry limit.</param>
        public CosmosDBConnectionChecker(Func<bool> getIsCosmosDBReady, ILogger logger, CancellationTokenSource cancellationTokenSource, int interval = 40, int retryLimit = 3)
        {
            this.getIsCosmosDBReady = getIsCosmosDBReady;
            this.retryLimit = retryLimit;
            this.logger = logger;
            this.cancellationTokenSource = cancellationTokenSource;

            this.InitDbChecker(interval);
        }

        /// <summary>
        /// Initializes the database checker.
        /// </summary>
        /// <param name="interval">The interval.</param>
        private void InitDbChecker(int interval)
        {
            this.dbConnectionHealthCheck = new ()
            {
                Interval = interval * 1000,
            };

            this.dbConnectionHealthCheck.Elapsed += this.DbConnectionHealthCheck_Elapsed;

            this.dbConnectionHealthCheck.Start();
        }

        /// <summary>
        /// Handles the Elapsed event of the DbConnectionHealthCheck control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Timers.ElapsedEventArgs"/> instance containing the event data.</param>
        private void DbConnectionHealthCheck_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            bool cosmosIsReady = this.getIsCosmosDBReady();

            // The max value of CosmosDbConnectionCheckRetries is expected to be equals to 3.
            if (this.failuresCount <= this.retryLimit && !cosmosIsReady)
            {
                // Retries twice more with decreasing interval between retries.
                this.dbConnectionHealthCheck.Stop();
                this.dbConnectionHealthCheck.Interval /= 2;
                this.dbConnectionHealthCheck.Start();

                this.logger.LogWarning(new EventId((int)LogLevel.Warning, nameof(this.DbConnectionHealthCheck_Elapsed)), $"DBConnection Health Check failed.  Attempt {this.failuresCount}/{this.retryLimit}.");

                this.failuresCount++;
            }
            else if (cosmosIsReady)
            {
                // reset dbCheckFailures Count.
                this.failuresCount = 1;
            }

            // reached out retry limit.
            if (this.failuresCount > this.retryLimit)
            {
                this.dbConnectionHealthCheck.Stop();

                this.logger.LogError(new EventId((int)LogLevel.Error, nameof(this.DbConnectionHealthCheck_Elapsed)), $"Unable to perform DBConnection Health Check after [{this.retryLimit}] attempts. Application will Terminate.");

                this.cancellationTokenSource.Cancel(false);
            }
        }
    }
}
