// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Extensions.Logging;

namespace LodeRunner.API.Middleware
{
    /// <summary>
    /// NGSA Logger Extensions.
    /// </summary>
    public static class NgsaLoggerExtensions
    {
        public static ILoggingBuilder AddNgsaLogger(this ILoggingBuilder builder, Action<NgsaLoggerConfiguration> configure)
        {
            NgsaLoggerConfiguration config = new ();
            configure(config);

            builder.AddProvider(new NgsaLoggerProvider(config));
            return builder;
        }
    }
}
