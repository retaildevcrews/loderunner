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
        /// The ASP net core environment.
        /// </summary>
        public const string AspNetCoreEnviroment = "ASPNETCORE_ENVIRONMENT";

        /// <summary>
        /// The AppSetting production environment name.
        /// </summary>
        public const string ProductionEnvironment = "Production";

        /// <summary>
        /// The AppSetting development environment name.
        /// </summary>
        public const string DevelopmentEnvironment = "Development";

        /// <summary>
        /// The command line validation duration and loop message.
        /// </summary>
        public const string CmdLineValidationDurationAndLoopMessage = "--run-loop must be true to use --duration";

        /// <summary>
        /// The command line validation random and loop message.
        /// </summary>
        public const string CmdLineValidationRandomAndLoopMessage = "--run-loop must be true to use --random";

        /// <summary>
        /// The command line validation delay start and empty secrets message.
        /// </summary>
        public const string CmdLineValidationDelayStartAndEmptySecretsMessage = "--secrets-volume cannot be empty when --delay-start is equals to -1";

        /// <summary>
        /// The command line validation secrets and invalid delay start message.
        /// </summary>
        public const string CmdLineValidationSecretsAndInvalidDelayStartMessage = "--secrets-volume requires --delay-start to be equals to negative one (-1)";

        /// <summary>
        /// The command line validation secrets volume beginning message.
        /// </summary>
        public const string CmdLineValidationSecretsVolumeBeginningMessage = "--secrets-volume (";

        /// <summary>
        /// The command line validation secrets volume end message.
        /// </summary>
        public const string CmdLineValidationSecretsVolumeEndMessage = ") does not exist";

        /// <summary>
        /// The exit success.
        /// </summary>
        public const int ExitSuccess = 0;

        /// <summary>
        /// The exit fail.
        /// </summary>
        public const int ExitFail = 1;
    }
}
