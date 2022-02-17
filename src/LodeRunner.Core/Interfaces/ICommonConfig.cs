// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;

namespace LodeRunner.Core.Interfaces
{
    /// <summary>
    /// Configurations for Logging.
    /// </summary>
    public interface ICommonConfig
    {
        /// <summary>
        /// Gets the log level.
        /// </summary>
        /// <value>
        /// The log level.
        /// </value>
        LogLevel LogLevel { get;  }

        /// <summary>
        /// Gets the request log level.
        /// </summary>
        /// <value>
        /// The request log level.
        /// </value>
        LogLevel RequestLogLevel { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is log level set.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is log level set; otherwise, <c>false</c>.
        /// </value>
        bool IsLogLevelSet { get;  }

        /// <summary>
        /// Gets the URL prefix.
        /// </summary>
        /// <value>
        /// The URL prefix.
        /// </value>
        string UrlPrefix { get; }
    }
}
