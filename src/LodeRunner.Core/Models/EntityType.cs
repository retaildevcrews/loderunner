// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LodeRunner.Core.Models
{
    /// <summary>
    /// Entity Types.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EntityType
    {
        /// <summary>
        /// Represents a front end compatible representation of ClientStatus
        /// </summary>
        Client,

        /// <summary>
        /// ClientStatus entity type
        /// </summary>
        ClientStatus,

        /// <summary>
        /// LoadTestConfig entity type
        /// </summary>
        LoadTestConfig,

        /// <summary>
        /// TestRun entity type
        /// </summary>
        TestRun,

        /// <summary>
        /// The load client
        /// </summary>
        LoadClient,

        /// <summary>
        /// Unassigned entity type
        /// </summary>
        Unassigned,
    }
}
