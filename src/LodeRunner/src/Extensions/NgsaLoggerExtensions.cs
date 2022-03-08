// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LodeRunner.Core;
using LodeRunner.Core.Interfaces;
using LodeRunner.Core.NgsaLogger;
using Microsoft.Extensions.Logging;

namespace LodeRunner.Extensions
{
    /// <summary>
    /// Represents a helper class to perform logging operations.
    /// </summary>
    internal static class NgsaLoggerExtensions
    {
        private static readonly object LoggingLock = new ();

        /// <summary>
        ///  Formats and writes a warning log message.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="config">The configuration.</param>
        /// <param name="ex">The ex.</param>
        /// <param name="message">The message.</param>
        /// <param name="methodName">Name of the method.</param>
        public static void NgsaLogWarnning(this ILogger logger, ILRConfig config, Exception ex, string message, [CallerMemberName] string methodName = null)
        {
            logger.NgsaScopeLogEntry(config, LogLevel.Warning, ex, message, methodName);
        }

        /// <summary>
        ///  Formats and writes an error log message.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="config">The configuration.</param>
        /// <param name="ex">The ex.</param>
        /// <param name="message">The message.</param>
        /// <param name="methodName">Name of the method.</param>
        public static void NgsaLogError(this ILogger logger, ILRConfig config, Exception ex, string message, [CallerMemberName] string methodName = null)
        {
            logger.NgsaScopeLogEntry(config, LogLevel.Error, ex, message, methodName);
        }

        /// <summary>
        ///  Formats and writes an informational log message.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="config">The configuration.</param>
        /// <param name="message">The message.</param>
        /// <param name="methodName">Name of the method.</param>
        public static void NgsaLogInformational(this ILogger logger, ILRConfig config, string message, [CallerMemberName] string methodName = null)
        {
            logger.NgsaScopeLogEntry(config, LogLevel.Information, null, message, methodName);
        }

        /// <summary>
        /// Begins the lode runner scope.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="properties">The properties.</param>
        /// <returns>Disposable.</returns>
        private static IDisposable BeginLodeRunnerScope(this ILogger logger, params ValueTuple<string, object>[] properties)
        {
            var dictionary = properties.ToDictionary(p => p.Item1, p => p.Item2);

            return logger.BeginScope(new LoggerDisposableScope(dictionary));
        }

        /// <summary>
        /// Logs the entry after updating Ngsa members ClientStatusId, LoadClientId, TestRunId.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="config">The configuration.</param>
        /// <param name="logLevel">The log level.</param>
        /// <param name="ex">The ex.</param>
        /// <param name="message">The message.</param>
        /// <param name="methodName">Name of the method.</param>
        private static void NgsaScopeLogEntry(this ILogger logger, ILRConfig config, LogLevel logLevel, Exception ex, string message, string methodName = null)
        {
            lock (LoggingLock)
            {
                using (logger.BeginLodeRunnerScope((SystemConstants.ClientStatusIdFieldName, config.ClientStatusId), (SystemConstants.LoadClientIdFieldName, config.LoadClientId), (SystemConstants.TestRunIdFieldName, config.TestRunId)))
                {
                    switch (logLevel)
                    {
                        case LogLevel.Error:
                            {
                                logger.LogError(new EventId((int)LogLevel.Error, methodName), ex, message);
                                break;
                            }

                        case LogLevel.Warning:
                            {
                                logger.LogWarning(new EventId((int)LogLevel.Warning, methodName), ex, message);
                                break;
                            }

                        default:
                            {
                                logger.LogInformation(new EventId((int)LogLevel.Information, methodName), ex, message);
                                break;
                            }
                    }
                }
            }
        }
    }
}
