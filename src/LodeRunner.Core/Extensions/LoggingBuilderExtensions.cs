// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json;
using LodeRunner.Core.Interfaces;
using LodeRunner.Core.NgsaLogger;
using Microsoft.Extensions.Logging;

namespace LodeRunner.Core.Extensions
{
    /// <summary>
    /// Provides ILogger Extensions.
    /// </summary>
    public static class LoggingBuilderExtensions
    {
        /// <summary>
        /// Configures ILoggingBuilder.
        /// </summary>
        /// <param name="logger">The ILoggingBuilder.</param>
        /// <param name="logLevelConfig">Log level configuration.</param>
        /// <param name="logValues">logValues interface than allows to inject a data dictionary when performing a log operation.</param>
        /// <param name="projectName">The project name.</param>
        /// <returns>ILoggingBuilder.</returns>
        public static ILoggingBuilder Setup(this ILoggingBuilder logger, ICommonConfig logLevelConfig, ILogValues logValues, string projectName)
        {
            // log to XML
            // this can be replaced when the dotnet XML logger is available
            logger.ClearProviders();

            logger.AddNgsaLogger(loggerConfig => { loggerConfig.LogLevel = logLevelConfig.LogLevel; }, logValues);

            // if you specify the --log-level option, it will override the appsettings.json options

            // remove any or all of the code below that you don't want to override
            if (logLevelConfig.IsLogLevelSet)
            {
                logger.AddFilter("Microsoft", logLevelConfig.LogLevel)
                .AddFilter("System", logLevelConfig.LogLevel)
                .AddFilter("Default", logLevelConfig.LogLevel)
                .AddFilter(projectName, logLevelConfig.LogLevel);
            }

            return logger;
        }
    }
}
