// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit.Abstractions;
using CommonTest = LodeRunner.API.Test.IntegrationTests.Common;

namespace LodeRunner.API.Test.IntegrationTests.ExecutingTestRun
{
    /// <summary>
    /// Represents the LodeRunner Client Mode Process Context Collection class.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    internal class LRClientModeProcessContextCollection : IDisposable
    {
        private readonly int instanceCount;
        private readonly string secretsVolume;
        private readonly ITestOutputHelper output;
        private bool disposedValue = false;
        private List<(int InstanceId, string ClientStatusId, ProcessContext LodeRunnerProcessContext)> processContextList;

        /// <summary>
        /// Initializes a new instance of the <see cref="LRClientModeProcessContextCollection"/> class.
        /// </summary>
        /// <param name="instanceCount">The LodeRunner Client Mode Instance count.</param>
        /// <param name="secretsVolume">The secrets volume.</param>
        /// <param name="output">The output.</param>
        public LRClientModeProcessContextCollection(int instanceCount, string secretsVolume, ITestOutputHelper output)
        {
            this.instanceCount = instanceCount;
            this.secretsVolume = secretsVolume;
            this.output = output;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="LRClientModeProcessContextCollection"/> class from being created.
        /// </summary>
        private LRClientModeProcessContextCollection()
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
        /// <returns>The Enumerator for LodeRunnerProcessContextCollection Tuple. </returns>
        public IEnumerator<(int InstanceId, string ClientStatusId, ProcessContext LodeRunnerProcessContext)> GetEnumerator()
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
        /// Starts a new LodeRunner Client mode Instance.
        /// </summary>
        /// <returns>whether or not the collection process started.</returns>
        public bool Start()
        {
            if (!this.Started)
            {
                this.processContextList = new();

                for (int instanceId = 1; instanceId <= this.instanceCount; instanceId++)
                {
                    var lodeRunnerContext = new ProcessContext(
                    new ProcessContextParams()
                    {
                        ProjectBasePath = "LodeRunner/LodeRunner.csproj",
                        ProjectArgs = $"--mode Client --secrets-volume {this.secretsVolume}",
                        ProjectBaseParentDirectoryName = "src",
                    },
                    this.output);

                    if (lodeRunnerContext.Start())
                    {
                        Task.Run(async () =>
                        {
                            // Get LodeRunner Client Status Id when Initializing Client
                            var clientStatusId = await CommonTest.ParseOutputGetFieldValueAndValidateIsNotNullOrEmpty(LodeRunner.Core.SystemConstants.LodeRunnerAppName, lodeRunnerContext.Output, LodeRunner.Core.SystemConstants.LodeRunnerServiceLogName, LodeRunner.Core.SystemConstants.InitializingClient, LodeRunner.Core.SystemConstants.ClientStatusIdFieldName, this.output, "Unable to get ClientStatusId when Initializing Client.");

                            this.processContextList.Add(new(instanceId, clientStatusId, lodeRunnerContext));
                        });
                    }
                    else
                    {
                        break;
                    }
                }

                // if the number of requested apiHostCount to be created is equals to the number of ProcessContext created and successfully started.
                this.Started = this.processContextList.Count == this.instanceCount;
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
                    foreach (var (instanceId, clientStatusId, lodeRunnerProcessContext) in this.processContextList)
                    {
                        lodeRunnerProcessContext.End();
                        this.output.WriteLine($"Stopping LodeRunner Client Mode for Instance: {instanceId}, ClientStatusId: {clientStatusId}");
                    }

                    // we remove any reference to the lodeRunnerProcess Context.
                    this.processContextList.Clear();

                    this.processContextList = null;
                }

                this.disposedValue = true;
            }
        }
    }
}
