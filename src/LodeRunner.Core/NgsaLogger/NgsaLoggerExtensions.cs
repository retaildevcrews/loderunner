// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using LodeRunner.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace LodeRunner.Core.NgsaLogger
{
    /// <summary>
    /// NGSA Logger Extensions.
    /// </summary>
    public static class NgsaLoggerExtensions
    {
        /// <summary>
        /// Add configured logger.
        /// </summary>
        /// <param name="builder">Logging builder.</param>
        /// <param name="configure">Log configuration.</param>
        /// <param name="logValues">logValues interface than allows to inject a new data dictionary when.</param>
        /// <returns>Logging <paramref name="builder"/>.</returns>
        public static ILoggingBuilder AddNgsaLogger(this ILoggingBuilder builder, Action<NgsaLoggerConfiguration> configure, ILogValues logValues)
        {
            NgsaLoggerConfiguration loggerConfig = new ();
            configure(loggerConfig);

            builder.AddProvider(new NgsaLoggerProvider(loggerConfig, logValues));

            return builder;
        }
    }
}
