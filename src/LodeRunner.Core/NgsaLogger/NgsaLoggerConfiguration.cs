// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;

namespace LodeRunner.Core.NgsaLogger
{
    /// <summary>
    /// NGSA Logger Configuration.
    /// </summary>
    public class NgsaLoggerConfiguration
    {
        /// <summary>
        /// Gets or sets the log level.
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Warning;
    }
}
