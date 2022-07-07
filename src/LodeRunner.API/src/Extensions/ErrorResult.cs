// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net;

namespace LodeRunner.API.Middleware
{
    /// <summary>
    /// Erred result.
    /// </summary>
    public class ErrorResult
    {
        /// <summary>
        /// Gets or sets erred result message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the erred result status code.
        /// </summary>
        public HttpStatusCode Error { get; set; }
    }
}
