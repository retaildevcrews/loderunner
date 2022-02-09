// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace LodeRunner.API.Test.IntegrationTests.ExecutingTestRun
{
    /// <summary>
    /// Provides and Manages a Process Context.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    internal class ProcessContext : IDisposable
    {
        private readonly string cmdLine;
        private readonly string args;
        private readonly ITestOutputHelper output;

        private bool disposedValue = false;
        private Process currentProcess;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessContext"/> class.
        /// </summary>
        /// <param name="cmdLine">Command Line.</param>
        /// <param name="args">The arguments.</param>
        /// <param name="output">The output.</param>
        public ProcessContext(string cmdLine, string args, ITestOutputHelper output)
        {
            this.args = args;
            this.cmdLine = cmdLine;
            this.output = output;
        }

        /// <summary>
        /// Gets the errors.
        /// </summary>
        /// <value>
        /// The errors.
        /// </value>
        public List<string> Errors { get; private set; } = new List<string>();

        /// <summary>
        /// Gets the output.
        /// </summary>
        /// <value>
        /// The output.
        /// </value>
        public List<string> Output { get; private set; } = new List<string>();

        /// <summary>
        /// Gets a value indicating whether this instance started.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance started; otherwise, <c>false</c>.
        /// </value>
        public bool Started { get; private set; } = false;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Starts the specified wait for exit.
        /// </summary>
        /// <returns>whether or not the process started.</returns>
        public bool Start()
        {
            string currentDir = Directory.GetCurrentDirectory();

            if (!this.Started)
            {
                this.currentProcess = new Process
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,

                        FileName = this.cmdLine,
                        Arguments = this.args,
                    },
                };

                this.currentProcess.StartInfo.RedirectStandardOutput = true;
                this.currentProcess.OutputDataReceived += this.CurrentProcess_OutputDataReceived;

                this.currentProcess.StartInfo.RedirectStandardError = true;
                this.currentProcess.ErrorDataReceived += this.CurrentProcess_ErrorDataReceived;

                this.Started = this.currentProcess.Start();

                this.currentProcess.BeginOutputReadLine();
                this.currentProcess.BeginErrorReadLine();
            }

            return this.Started;
        }

        /// <summary>
        /// Ends this instance.
        /// </summary>
        public void End()
        {
            this.Dispose();
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.currentProcess?.Kill(true);
                }

                this.disposedValue = true;
            }
        }

        /// <summary>
        /// Handles the ErrorDataReceived event of the CurrentProcess control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataReceivedEventArgs"/> instance containing the event data.</param>
        private void CurrentProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e?.Data))
            {
                this.Errors.Add(e?.Data);
                this.output.WriteLine(e?.Data);
            }
        }

        /// <summary>
        /// Handles the OutputDataReceived event of the CurrentProcess control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataReceivedEventArgs"/> instance containing the event data.</param>
        private void CurrentProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e?.Data))
            {
                this.Output.Add(e?.Data);
                this.output.WriteLine(e?.Data);
            }
        }
    }
}
