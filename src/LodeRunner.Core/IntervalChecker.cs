// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LodeRunner.Core
{
    /// <summary>
    /// Helper class to perform a boolean check to pointer function on a given interval.
    /// </summary>
    public class IntervalChecker : IDisposable
    {
        private readonly ILogger logger;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly int retryLimit = 3;

        private readonly Func<Task<bool>> getBooleanCheck;
        private int failuresCount = 1;
        private System.Timers.Timer intervalCheckTimer = default;
        private bool started = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntervalChecker"/> class.
        /// </summary>
        /// <param name="getBooleanCheck">The boolean function pointer to call, that must return 'true' indicating that the check passed and 'false' otherwise.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <param name="interval">The interval (secs).</param>
        /// <param name="retryLimit">The retry limit.</param>
        public IntervalChecker(Func<Task<bool>> getBooleanCheck, ILogger logger, CancellationTokenSource cancellationTokenSource, int interval = 60, int retryLimit = 3)
        {
            this.getBooleanCheck = getBooleanCheck;
            this.retryLimit = retryLimit;
            this.logger = logger;
            this.cancellationTokenSource = cancellationTokenSource;

            this.InitCheckTimer(interval);
        }

        /// <summary>
        /// Starts the timer check.
        /// </summary>
        public void Start()
        {
            if (this.started)
            {
                return;
            }

            this.intervalCheckTimer.Start();

            this.started = true;
        }

        /// <summary>
        /// Stops this timer check.
        /// </summary>
        public void Stop()
        {
            this.intervalCheckTimer.Stop();
            this.started = false;
        }

        /// <summary>
        /// Performs class-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Stop();
            this.intervalCheckTimer = null;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Initializes the timer.
        /// </summary>
        /// <param name="interval">The interval.</param>
        private void InitCheckTimer(int interval)
        {
            this.intervalCheckTimer = new ()
            {
                Interval = interval * 1000,
            };

            this.intervalCheckTimer.Elapsed += this.CheckElapsed;
        }

        /// <summary>
        /// Handles the Elapsed event of the Check control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Timers.ElapsedEventArgs"/> instance containing the event data.</param>
        private void CheckElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            bool checkResult = this.getBooleanCheck().Result;

            if (this.failuresCount <= this.retryLimit && !checkResult)
            {
                // Retries twice more with decreasing interval between retries.
                this.intervalCheckTimer.Stop();
                this.intervalCheckTimer.Interval /= 2;
                this.intervalCheckTimer.Start();

                this.logger.LogWarning(new EventId((int)LogLevel.Warning, nameof(this.CheckElapsed)), string.Format(Core.SystemConstants.IntervalCheckFailedAttemptMessage, this.failuresCount, this.retryLimit));

                this.failuresCount++;
            }
            else if (checkResult)
            {
                // reset failures Count.
                this.failuresCount = 1;
            }

            // reached out retry limit.
            if (this.failuresCount > this.retryLimit)
            {
                this.intervalCheckTimer.Stop();

                this.logger.LogError(new EventId((int)LogLevel.Error, nameof(this.CheckElapsed)), string.Format(Core.SystemConstants.IntervalCheckErrorMessage, this.retryLimit));

                this.cancellationTokenSource.Cancel(false);
            }
        }
    }
}