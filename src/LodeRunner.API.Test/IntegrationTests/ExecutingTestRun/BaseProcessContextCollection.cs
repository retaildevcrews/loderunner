// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Xunit.Abstractions;

namespace LodeRunner.API.Test.IntegrationTests.ExecutingTestRun
{
    /// <summary>
    /// Represents the Api Process Context Collection class.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    internal class BaseProcessContextCollection : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseProcessContextCollection"/> class.
        /// </summary>
        /// <param name="instanceCount">The API host count.</param>
        /// <param name="secretsVolume">The secrets volume.</param>
        /// <param name="output">The output.</param>
        public BaseProcessContextCollection(int instanceCount, string secretsVolume, ITestOutputHelper output)
        {
            this.InstanceCount = instanceCount;
            this.SecretsVolume = secretsVolume;
            this.Output = output;
        }

        ///// <summary>
        ///// Prevents a default instance of the <see cref="BaseProcessContextCollection"/> class from being created.
        ///// </summary>
        private BaseProcessContextCollection()
        {
        }

        /// <summary>
        /// Gets a value for Instance Count.
        /// </summary>
        public int InstanceCount { get; private set; }

        /// <summary>
        /// Gets a value  for Sectrest.
        /// </summary>
        public string SecretsVolume { get; private set; }

        /// <summary>
        /// Gets the Test Output object.
        /// </summary>
        public ITestOutputHelper Output { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether indicates if proptected Dispose method was called.
        /// </summary>
        public bool DisposedValue { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance started.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance started; otherwise, <c>false</c>.
        /// </value>
        public bool Started { get; protected set; } = false;

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
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
           throw new NotImplementedException();
        }
    }
}
