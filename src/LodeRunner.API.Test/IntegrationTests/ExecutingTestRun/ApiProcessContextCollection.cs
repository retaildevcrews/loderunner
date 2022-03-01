// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Xunit.Abstractions;

namespace LodeRunner.API.Test.IntegrationTests.ExecutingTestRun
{
    /// <summary>
    /// Represents the Api Process Context Collection class.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    internal class ApiProcessContextCollection : IDisposable
    {
        private readonly int apiHostCount;
        private readonly string secretsVolume;
        private readonly ITestOutputHelper output;
        private bool disposedValue = false;
        private List<(int hostId, int portNumber, ProcessContext apiProcessContext)> processContextList;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiProcessContextCollection"/> class.
        /// </summary>
        /// <param name="apiHostCount">The API host count.</param>
        /// <param name="secretsVolume">The secrets volume.</param>
        /// <param name="output">The output.</param>
        public ApiProcessContextCollection(int apiHostCount, string secretsVolume, ITestOutputHelper output)
        {
            this.apiHostCount = apiHostCount;
            this.secretsVolume = secretsVolume;
            this.output = output;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="ApiProcessContextCollection"/> class from being created.
        /// </summary>
        private ApiProcessContextCollection()
        {
        }

        /// <summary>
        /// Gets a value indicating whether this instance started.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance started; otherwise, <c>false</c>.
        /// </value>
        public bool Started { get; private set; } = false;

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>The Enumerator for ApiProcessContextCollection Tuple. </returns>
        public IEnumerator<(int hostId, int portNumber, ProcessContext apiProcessContext)> GetEnumerator()
        {
            foreach (var processContext in this.processContextList)
            {
                yield return processContext;
            }
        }

        /// <summary>
        /// Ends this instance.
        /// </summary>
        public void End()
        {
            this.Dispose();
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
        /// Starts the specified next available port.
        /// </summary>
        /// <param name="getNextAvailablePort">The next available port.</param>
        /// <returns>whether or not the collection process started.</returns>
        public bool Start(Func<int> getNextAvailablePort)
        {
            if (!this.Started)
            {
                this.processContextList = new ();

                for (int hostId = 1; hostId <= this.apiHostCount; hostId++)
                {
                    int apiPortNumber = getNextAvailablePort();
                    var lodeRunnerAPIContext = new ProcessContext(
                    new ProcessContextParams()
                    {
                        ProjectBasePath = "LodeRunner.API/LodeRunner.API.csproj",
                        ProjectArgs = $"--port {apiPortNumber} --secrets-volume {this.secretsVolume}",
                        ProjectBaseParentDirectoryName = "src",
                    }, this.output);

                    if (lodeRunnerAPIContext.Start())
                    {
                        this.processContextList.Add(new (hostId, apiPortNumber, lodeRunnerAPIContext));
                    }
                    else
                    {
                        break;
                    }
                }

                // if the number of requested apiHostCount to be created is equals to the number of ProcessContext created and successfully started.
                this.Started = this.processContextList.Count == this.apiHostCount;
            }

            return this.Started;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    foreach (var apiProcessContext in this.processContextList)
                    {
                        apiProcessContext.apiProcessContext.End();
                        this.output.WriteLine($"Stopping LodeRunner API for Host {apiProcessContext.hostId}.");
                    }

                    // we remove any reference to the ApiProcess Context.
                    this.processContextList.Clear();

                    this.processContextList = null;
                }

                this.disposedValue = true;
            }
        }
    }
}
