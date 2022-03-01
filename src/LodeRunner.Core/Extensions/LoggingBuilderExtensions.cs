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
        /// <param name="config">The configuration.</param>
        /// <param name="projectName">The project name.</param>
        /// <returns>ILoggingBuilder.</returns>
        public static ILoggingBuilder Setup(this ILoggingBuilder logger, ICommonConfig config, string projectName)
        {
            // log to XML
            // this can be replaced when the dotnet XML logger is available
            logger.ClearProviders();

            logger.AddNgsaLogger(loggerConfig => { loggerConfig.LogLevel = config.LogLevel; });

            // if you specify the --log-level option, it will override the appsettings.json options

            // remove any or all of the code below that you don't want to override
            if (config.IsLogLevelSet)
            {
                logger.AddFilter("Microsoft", config.LogLevel)
                .AddFilter("System", config.LogLevel)
                .AddFilter("Default", config.LogLevel)
                .AddFilter(projectName, config.LogLevel);
            }

            return logger;
        }
    }
}
