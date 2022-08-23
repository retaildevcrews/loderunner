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
    internal class ApiProcessContextCollection : BaseProcessContextCollection
    {
        private List<(int HostId, int PortNumber, ProcessContext ApiProcessContext)> processContextList;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiProcessContextCollection"/> class.
        /// </summary>
        /// <param name="apiHostCount">The API host count.</param>
        /// <param name="secretsVolume">The secrets volume.</param>
        /// <param name="output">The output.</param>
        public ApiProcessContextCollection(int apiHostCount, string secretsVolume, ITestOutputHelper output)
            : base(apiHostCount, secretsVolume, output)
        {
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>The Enumerator for ApiProcessContextCollection Tuple. </returns>
        public IEnumerator<(int HostId, int PortNumber, ProcessContext ApiProcessContext)> GetEnumerator()
        {
            foreach (var processContext in this.processContextList)
            {
                yield return processContext;
            }
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
                this.processContextList = new();

                for (int hostId = 1; hostId <= this.InstanceCount; hostId++)
                {
                    int apiPortNumber = getNextAvailablePort();
                    var lodeRunnerAPIContext = new ProcessContext(
                    new ProcessContextParams()
                    {
                        ProjectBasePath = "LodeRunner.API/LodeRunner.API.csproj",
                        ProjectArgs = $"--port {apiPortNumber} --secrets-volume {this.SecretsVolume}",
                        ProjectBaseParentDirectoryName = "src",
                    },
                    this.Output);

                    if (lodeRunnerAPIContext.Start())
                    {
                        this.processContextList.Add(new(hostId, apiPortNumber, lodeRunnerAPIContext));
                    }
                    else
                    {
                        break;
                    }
                }

                // if the number of requested apiHostCount to be created is equals to the number of ProcessContext created and successfully started.
                this.Started = this.processContextList.Count == this.InstanceCount;
            }

            return this.Started;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!this.DisposedValue)
            {
                if (disposing)
                {
                    foreach (var (hostId, portNumber, apiProcessContext) in this.processContextList)
                    {
                        apiProcessContext.End();
                        this.Output.WriteLine($"Stopping LodeRunner API for Host {hostId}:{portNumber}");
                    }

                    // we remove any reference to the ApiProcess Context.
                    this.processContextList.Clear();

                    this.processContextList = null;
                }

                this.DisposedValue = true;
            }
        }
    }
}
