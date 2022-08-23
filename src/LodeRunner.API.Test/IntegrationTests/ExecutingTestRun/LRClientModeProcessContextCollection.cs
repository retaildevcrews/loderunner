// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace LodeRunner.API.Test.IntegrationTests.ExecutingTestRun
{
    /// <summary>
    /// Represents the LodeRunner Client Mode Process Context Collection class.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    internal class LRClientModeProcessContextCollection : BaseProcessContextCollection
    {
        private List<(int InstanceId, ProcessContext LodeRunnerProcessContext)> processContextList;

        /// <summary>
        /// Initializes a new instance of the <see cref="LRClientModeProcessContextCollection"/> class.
        /// </summary>
        /// <param name="instanceCount">The LodeRunner Client Mode Instance count.</param>
        /// <param name="secretsVolume">The secrets volume.</param>
        /// <param name="output">The output.</param>
        public LRClientModeProcessContextCollection(int instanceCount, string secretsVolume, ITestOutputHelper output)
            : base(instanceCount, secretsVolume, output)
        {
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>The Enumerator for LodeRunnerProcessContextCollection Tuple. </returns>
        public IEnumerator<(int InstanceId, ProcessContext LodeRunnerProcessContext)> GetEnumerator()
        {
            foreach (var processContext in this.processContextList)
            {
                yield return processContext;
            }
        }

        /// <summary>
        /// Get First or Default instance.
        /// </summary>
        /// <returns>The first or default instance created.</returns>
        public ProcessContext FirstOrDefault()
        {
            return this.processContextList.FirstOrDefault().LodeRunnerProcessContext;
        }

        /// <summary>
        /// Starts a new LodeRunner Client mode Instance.
        /// </summary>
        /// <returns>whether or not the collection process started.</returns>
        public bool Start()
        {
            if (!this.Started)
            {
                this.processContextList = new();

                for (int instanceId = 1; instanceId <= this.InstanceCount; instanceId++)
                {
                    var lodeRunnerContext = new ProcessContext(
                    new ProcessContextParams()
                    {
                        ProjectBasePath = "LodeRunner/LodeRunner.csproj",
                        ProjectArgs = $"--mode Client --secrets-volume {this.SecretsVolume}",
                        ProjectBaseParentDirectoryName = "src",
                    },
                    this.Output);

                    if (lodeRunnerContext.Start())
                    {
                        this.processContextList.Add(new(instanceId, lodeRunnerContext));
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
                    foreach (var (instanceId, lodeRunnerProcessContext) in this.processContextList)
                    {
                        lodeRunnerProcessContext.End();
                        this.Output.WriteLine($"Stopping LodeRunner Client Mode for Instance: {instanceId}");
                    }

                    // we remove any reference to the lodeRunnerProcess Context.
                    this.processContextList.Clear();

                    this.processContextList = null;
                }

                this.DisposedValue = true;
            }
        }
    }
}
