// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace LodeRunner.API
{
    /// <summary>
    /// System constants.
    /// </summary>
    public class SystemConstants
    {
        /// <summary>
        /// Status code of error.
        /// </summary>
        public const string Error = "Error";

        /// <summary>
        /// Status code of retry.
        /// </summary>
        public const string Retry = "Retry";

        /// <summary>
        /// Status code of retry.
        /// </summary>
        public const string Warn = "Warn";

        /// <summary>
        /// Status code of OK.
        /// </summary>
        public const string OK = "OK";

        /// <summary>
        /// Forwarded for header.
        /// </summary>
        public const string XffHeader = "X-Forwarded-For";

        /// <summary>
        /// Client IP header.
        /// </summary>
        public const string IpHeader = "X-Client-IP";
    }
}
