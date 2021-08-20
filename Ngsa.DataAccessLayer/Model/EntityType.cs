// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ngsa.LodeRunner.DataAccessLayer.Model
{
    /// <summary>
    /// Entity Types.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EntityType
    {
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
    }
}
