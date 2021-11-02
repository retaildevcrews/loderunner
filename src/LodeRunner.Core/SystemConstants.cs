// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LodeRunner.Core
{
    /// <summary>
    /// System Constants class for Data.
    /// </summary>
    public class SystemConstants
    {
        /// <summary>
        /// The client status expiration time.
        /// </summary>
        public const int ClientStatusExpirationTime = 60;

        /// <summary>
        /// The unknown constant used for Region or Zone.
        /// </summary>
        public const string Unknown = "Unknown";

        /// <summary>
        /// The lode runner port setting name.
        /// </summary>
        public const string LodeRunnerPortSettingName = "LodeRunnerPort";

        /// <summary>
        /// The lode runner API port setting name.
        /// </summary>
        public const string LodeRunnerAPIPortSettingName = "LodeRunnerAPIPort";

        /// <summary>
        /// The lode runner UI port setting name.
        /// </summary>
        public const string LodeRunnerUIPortSettingName = "LodeRunnerUIPort";

        /// <summary>
        /// The ASP net core enviroment.
        /// </summary>
        public const string AspNetCoreEnviroment = "ASPNETCORE_ENVIRONMENT";
    }
}
