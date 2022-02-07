using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LodeRunner.API.Test.IntegrationTests.ExecutingTestRun
{
    /// <summary>
    /// Provides and Manages a Process Context.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    internal class ProcessContext : IDisposable
    {

        private readonly string pathFileName;
        private readonly string args;

        private bool disposedValue = false;
        private bool isRunning = false;
        private Process currentProcess;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessContext"/> class.
        /// </summary>
        /// <param name="pathFileName">Name of the path file.</param>
        /// <param name="args">The arguments.</param>
        public ProcessContext(string pathFileName, string args)
        {
            this.args = args;
            this.pathFileName = pathFileName;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        /// <returns>whether or not the process started.</returns>
        public bool Start()
        {
            if (!this.isRunning)
            {
                var startInfo = new ProcessStartInfo()
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,

                    FileName = this.pathFileName,
                    Arguments = this.args,
                };

                this.currentProcess = Process.Start(startInfo);

                this.isRunning = this.currentProcess != null;
            }

            return this.currentProcess != null;
        }

        /// <summary>
        /// Ends this instance.
        /// </summary>
        public void End()
        {
            this.currentProcess.Kill(true);
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
    }
}
