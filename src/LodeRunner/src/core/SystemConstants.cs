// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace LodeRunner
{
    public static class SystemConstants
    {
        public const int ExitSuccess = 0;
        public const int ExitFail = 1;

        public const string CmdLineValidationDurationAndLoopMessage = "--run-loop must be true to use --duration";

        public const string CmdLineValidationRandomAndLoopMessage = "--run-loop must be true to use --random";

        public const string CmdLineValidationDelayStartAndEmptySecretsMessage = "--secrets-volume cannot be empty when --delay-start is equals to -1";

        public const string CmdLineValidationSecretsAndInvalidDelayStartMessage = "--secrets-volume requires --delay-start to be equals to negative one (-1)";

        public const string CmdLineValidationSecretsVolumeBeginningMessage = "--secrets-volume (";

        public const string CmdLineValidationSecretsVolumeEndMessage = ") does not exist";

        public const string Unknown = "Unknown";
    }
}
