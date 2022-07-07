// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace LodeRunner.API.Middleware
{
    /// <summary>
    /// Logger options used to configure DI.
    /// </summary>
    public class RequestLoggerOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether enabled 200 logging.
        /// </summary>
        public bool Log2xx { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether enabled 300 logging.
        /// </summary>
        public bool Log3xx { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether enabled 400 logging.
        /// </summary>
        public bool Log4xx { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether enabled 500 logging.
        /// </summary>
        public bool Log5xx { get; set; } = true;

        /// <summary>
        /// Gets or sets a value for target ms.
        /// </summary>
        public double TargetMs { get; set; } = 1000;
    }
}
