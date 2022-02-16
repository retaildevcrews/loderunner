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
        private readonly ProcessContextParams processContextParams;
        private readonly ITestOutputHelper output;

        private bool disposedValue = false;
        private Process currentProcess;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessContext"/> class.
        /// </summary>
        /// <param name="processContextParams">The Process Context Params.</param>
        /// <param name="output">The output.</param>
        public ProcessContext(ProcessContextParams processContextParams, ITestOutputHelper output)
        {
            this.processContextParams = processContextParams;
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
        /// Starts the specified Process.
        /// </summary>
        /// <param name="delayReturn">delay Return (ms).</param>
        /// <returns>whether or not the process started.</returns>
        public bool Start(int delayReturn = 0)
        {
            if (!this.Started)
            {
                this.currentProcess = new Process
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,

                        FileName = this.processContextParams.CommandLine,
                        Arguments = this.BuildAndGetArguments(),
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

            Task.Delay(delayReturn).ConfigureAwait(false); // Let the APIs run for some time

            return this.Started;
        }

        /// <summary>
        /// Ends this instance.
        /// </summary>
        public void End()
        {
            this.Dispose();
        }

        /// <summary>
        /// Gets the project relative path by referencing parent directory name from CurrentDirectory.
        /// </summary>
        /// <param name="baseProjectPath">The base project path.</param>
        /// <param name="parentDirectoryName">The source Directory Name.</param>
        /// <returns>the relative project path.</returns>
        private string GetProjectRelativePathByReferencingParentDirName(string baseProjectPath, string parentDirectoryName)
        {
            string result;

            List<string> subDirList;

            string dirName = System.Environment.CurrentDirectory;
            this.output.WriteLine($"CurrentDirectory: {dirName}");

            // Identifies how many folder above is "sourceDirectoryName" are and replace them with relative path "../"
            if (System.OperatingSystem.IsLinux())
            {
                subDirList = System.Environment.CurrentDirectory.Split("/").ToList();
            }
            else
            {
                subDirList = System.Environment.CurrentDirectory.Split("\\").ToList();
            }

            int parentDirIndx = subDirList.IndexOf(parentDirectoryName);

            if (parentDirIndx > 0)
            {
                // We need to subtract 1 since we do not want to include the parentDirName in the relative path building process.
                int realtivePathSeparatorCount = subDirList.Count - parentDirIndx - 1;

                string relativePathPrefix = string.Concat(Enumerable.Repeat("../", realtivePathSeparatorCount));

                result = $"{relativePathPrefix}{baseProjectPath.TrimStart(new char[] { '/' })}";
            }
            else
            {
                // return basePath with a '/' prefix.
                result = $"/{baseProjectPath.TrimStart(new char[] { '/' })}";
            }

            this.output.WriteLine($"Relative Path: {result}");

            return result;
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
        /// Builds and Gets the arguments from processContextParams.
        /// </summary>
        /// <returns>the arguments string.</returns>
        private string BuildAndGetArguments()
        {
            string result = $"{this.processContextParams.CommandLineArgs} {this.GetProjectRelativePathByReferencingParentDirName(this.processContextParams.ProjectBasePath, this.processContextParams.ProjectBaseParentDirectoryName)} {this.processContextParams.ProjectArgs}";

            return result;
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
