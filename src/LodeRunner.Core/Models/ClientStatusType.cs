// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LodeRunner.Core.Models
{
    /// <summary>
    /// Client Status Types.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ClientStatusType
    {
        /// <summary>
        /// Starting client status
        /// </summary>
        Starting,

        /// <summary>
        /// Ready client status
        /// </summary>
        Ready,

        /// <summary>
        /// Testing client status
        /// </summary>
        Testing,

        /// <summary>
        /// Terminating client status
        /// </summary>
        Terminating,
    }
}
