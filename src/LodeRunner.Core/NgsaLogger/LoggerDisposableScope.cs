// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LodeRunner.Core.NgsaLogger
{
    /// <summary>
    /// Represents Logger Disposable Scope class.
    /// </summary>
    public class LoggerDisposableScope : IDisposable
    {
        private bool disposedValue = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerDisposableScope"/> class.
        /// </summary>
        /// <param name="context">The logging context.</param>
        public LoggerDisposableScope(Dictionary<string, object> context)
        {
            this.Context = context;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="LoggerDisposableScope"/> class from being created.
        /// </summary>
        private LoggerDisposableScope()
        {
        }

        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>
        /// The context.
        /// </value>
        public Dictionary<string, object> Context { get; private set; }

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
        private void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                     // we remove any reference to the dictionary Context.
                    this.Context.Clear();

                    this.Context = null;
                }

                this.disposedValue = true;
            }
        }
    }
}
