// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using LodeRunner.Core.NgsaLogger;
using LodeRunner.Model;
using Microsoft.Extensions.Logging;

namespace LodeRunner
{
    /// <summary>
    /// Shared state for the Timer Request Tasks.
    /// </summary>
    internal class TimerRequestState : IDisposable
    {
        private static Semaphore loopController;
        private readonly ILogger logger;
        private System.Timers.Timer timer;
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimerRequestState"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public TimerRequestState(ILogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// gets or sets the server name.
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// gets or sets the http client to use.
        /// </summary>
        public HttpClient Client { get; set; }

        /// <summary>
        /// gets or sets the request index.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// gets or sets the max request index.
        /// </summary>
        public int MaxIndex { get; set; }

        /// <summary>
        /// gets or sets the count.
        /// </summary>
        public long Count { get; set; }

        /// <summary>
        /// gets or sets the duration in ms.
        /// </summary>
        public double Duration { get; set; }

        /// <summary>
        /// gets or sets the number of errors.
        /// </summary>
        public int ErrorCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use random requests.
        /// </summary>
        public bool Random { get; set; }

        /// <summary>
        /// gets the lock object.
        /// </summary>
        public object Lock { get; } = new object();

        /// <summary>
        /// gets or sets the lode runner object.
        /// </summary>
        public ValidationTest Test { get; set; }

        /// <summary>
        /// gets or sets the current date time.
        /// </summary>
        public DateTime CurrentLogTime { get; set; }

        /// <summary>
        /// gets or sets the cancellation token.
        /// </summary>
        public CancellationToken Token { get; set; }

        /// <summary>
        /// Gets or sets the request list.
        /// </summary>
        /// <value>
        /// The request list.
        /// </value>
        public List<Request> RequestList { get; set; }

        /// <summary>
        /// Runs the specified interval.
        /// </summary>
        /// <param name="interval">The interval.</param>
        /// <param name="maxConcurrent">The maximum concurrent.</param>
        public void Run(double interval, int maxConcurrent)
        {
            loopController = new Semaphore(maxConcurrent, maxConcurrent);

            this.timer = new System.Timers.Timer(interval)
            {
                Enabled = true,
            };
            this.timer.Elapsed += this.TimerEvent;
            this.timer.Start();
        }

        /// <summary>
        /// Timers the event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Timers.ElapsedEventArgs"/> instance containing the event data.</param>
        private async void TimerEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            int index = 0;

            // exit if cancelled
            if (this.Token.IsCancellationRequested)
            {
                this.timer.Stop();
                this.timer.Dispose();
                this.timer = null;
                this.Client.Dispose();
                this.Client = null;

                return;
            }

            // verify http client
            if (this.Client == null)
            {
                Console.WriteLine($"{ValidationTest.Now}\tError\tTimerState http client is null");
                return;
            }

            // get a semaphore slot - rate limit the requests
            if (!loopController.WaitOne(10))
            {
                return;
            }

            // lock the state for updates
            lock (this.Lock)
            {
                index = this.Index;

                // increment
                this.Index++;

                // keep the index in range
                if (this.Index >= this.MaxIndex)
                {
                    this.Index = 0;
                }
            }

            // randomize request index
            if (this.Random)
            {
                index = DateTime.UtcNow.Millisecond % this.MaxIndex;
            }

            Request req = this.RequestList[index];

            try
            {
                // Execute the request
                PerfLog p = await this.Test.ExecuteRequest(this.Client, this.Server, req).ConfigureAwait(false);

                lock (this.Lock)
                {
                    // increment
                    this.Count++;
                    this.ErrorCount += p.ErrorCount;
                    this.Duration += p.Duration;
                }
            }
            catch (Exception ex)
            {
                // log and ignore any error
                this.logger.LogError(new EventId((int)EventTypes.CommonEvents.Exception, "LodeRunnerException"), ex, $"{ValidationTest.Now}\t{ex.Message}");
            }

            // make sure to release the semaphore
            loopController.Release();
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:Elements should be ordered by access", Justification = "grouping IDispose methods")]
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    if (this.timer != null)
                    {
                        this.timer.Stop();
                        this.timer.Dispose();
                    }
                }

                this.disposedValue = true;
            }
        }
    }
}
