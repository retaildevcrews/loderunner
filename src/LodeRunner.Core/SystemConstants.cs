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
        /// The command line validation random and loop message.
        /// </summary>
        public const string CmdLineValidationRandomAndLoopMessage = "--run-loop must be true to use --random";

        /// <summary>
        /// The command line validation delay start and empty secrets message.
        /// </summary>
        public const string CmdLineValidationClientModeAndEmptySecretsMessage = "--secrets-volume cannot be empty when --mode is equals to Client";

        /// <summary>
        /// The command line validation secrets volume beginning message.
        /// </summary>
        public const string CmdLineValidationSecretsVolumeBeginningMessage = "--secrets-volume (";

        /// <summary>
        /// The command line validation secrets volume end message.
        /// </summary>
        public const string CmdLineValidationSecretsVolumeEndMessage = ") does not exist";

        /// <summary>
        /// The command line validation max error and loop message.
        /// </summary>
        public const string CmdLineValidationMaxErrorAndLoopMessage = "--max-errors cannot be set when --run-loop is enabled";

        /// <summary>
        /// The command line notice message that sleep value is ignored.
        /// </summary>
        public const string CmdLineNoticeSleepValueIgnoredMessage = "--sleep value of 0 is ignored while --run-loop is also set";

        /// <summary>
        /// The command line notice message that duration value is ignored.
        /// </summary>
        public const string CmdLineNoticeDurationValueIgnoredMessage = "--duration value of 0 is ignored while --run-loop is also set";

        /// <summary>
        /// The exit success.
        /// </summary>
        public const int ExitSuccess = 0;

        /// <summary>
        /// The exit fail.
        /// </summary>
        public const int ExitFail = 1;

        /// <summary>
        /// The LodeRunner Client mode.
        /// </summary>
        public const string LodeRunnerClientMode = "Client";

        /// <summary>
        /// The LodeRunner Command mode.
        /// </summary>
        public const string LodeRunnerCommandMode = "Command";

        /// <summary>
        /// The initializing client.
        /// </summary>
        public const string InitializingClient = "Initializing Client";

        /// <summary>
        /// The client ready.
        /// </summary>
        public const string ClientReady = "Client Ready";

        /// <summary>
        /// The received new test run.
        /// </summary>
        public const string ReceivedNewTestRun = "Received new TestRun";

        /// <summary>
        /// The executing test run.
        /// </summary>
        public const string ExecutingTestRun = "Executing TestRun";

        /// <summary>
        /// The terminating client.
        /// </summary>
        public const string TerminatingClient = "Terminating Client";

        /// <summary>
        /// The default API web host port.
        /// </summary>
        public const int DefaultApiWebHostPort = 8080;

        /// <summary>
        /// The client status id field name.
        /// </summary>
        public const string ClientStatusIdFieldName = "ClientStatusId";

        /// <summary>
        /// The test run id field name.
        /// </summary>
        public const string TestRunIdFieldName = "TestRunId";

        /// <summary>
        /// The load client identifier field name.
        /// </summary>
        public const string LoadClientIdFieldName = "LoadClientId";

        /// <summary>
        /// The task canceled exception string.
        /// </summary>
        public const string TaskCanceledException = "Task Canceled Exception";

        /// <summary>
        /// The operation canceled exception string.
        /// </summary>
        public const string OperationCanceledException = "Operation Canceled Exception";

        /// <summary>
        /// The cosmos exception string.
        /// </summary>
        public const string CosmosException = "Cosmos Exception";

        /// <summary>
        /// The exception string.
        /// </summary>
        public const string Exception = "Exception";

        /// <summary>
        /// The polling test runs string.
        /// </summary>
        public const string PollingTestRuns = "Polling for available TestRuns";

        /// <summary>
        /// The shutting down string.
        /// </summary>
        public const string ShuttingDown = "Shutting down";

        /// <summary>
        /// The load test request log name.
        /// </summary>
        public const string LoadTestRequestLogName = "LoadTestRequest";
    }
}
