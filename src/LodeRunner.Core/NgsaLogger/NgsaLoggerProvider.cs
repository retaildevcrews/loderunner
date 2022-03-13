// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using LodeRunner.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace LodeRunner.Core.NgsaLogger
{
    /// <summary>
    /// NGSA Logger Provider.
    /// </summary>
    public sealed class NgsaLoggerProvider : ILoggerProvider
    {
        private readonly NgsaLoggerConfiguration config;
        private readonly ConcurrentDictionary<string, NgsaLogger> loggers = new ();
        private readonly ILogValues logValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="NgsaLoggerProvider"/> class.
        /// </summary>
        /// <param name="loggerConfig">NgsaLoggerConfig.</param>
        /// <param name="logValues">logValues interface than allows to inject a new data dictionary when.</param>
        public NgsaLoggerProvider(NgsaLoggerConfiguration loggerConfig, ILogValues logValues)
        {
            this.config = loggerConfig;
            this.logValues = logValues;
        }

        /// <summary>
        /// Create a logger by category name (usually assembly).
        /// </summary>
        /// <param name="categoryName">Category Name.</param>
        /// <returns>ILogger.</returns>
        public ILogger CreateLogger(string categoryName)
        {
            NgsaLogger logger = this.loggers.GetOrAdd(categoryName, new NgsaLogger(categoryName, this.config, this.logValues));
            return logger;
        }

        /// <summary>
        /// IDispose.Dispose().
        /// </summary>
        public void Dispose()
        {
            this.loggers.Clear();
        }
    }
}
