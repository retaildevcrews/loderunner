// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using LodeRunner.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.CorrelationVector;
using Microsoft.Extensions.Logging;
using CorrelationVectorExtensions = LodeRunner.Core.Extensions.CorrelationVectorExtensions;

namespace LodeRunner.Core.NgsaLogger
{
    /// <summary>
    /// Simple aspnet core middleware that logs requests to the console.
    /// </summary>
    public class NgsaLogger : ILogger
    {
        private readonly string name;
        private readonly NgsaLoggerConfiguration config;
        private readonly ILogValues logValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="NgsaLogger"/> class.
        /// </summary>
        /// <param name="name">Logger Name.</param>
        /// <param name="config">Logger Config.</param>
        /// <param name="logValues">logValues interface than allows to inject a new data dictionary when.</param>
        public NgsaLogger(string name, NgsaLoggerConfiguration config, ILogValues logValues)
        {
            this.name = name;
            this.config = config;
            this.logValues = logValues;
        }

        /// <summary>
        /// Gets or sets zone.
        /// </summary>
        public static string Zone { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets region.
        /// </summary>
        public static string Region { get; set; } = string.Empty;

        /// <summary>
        /// Creates the scope.
        /// </summary>
        /// <typeparam name="TState">State type.</typeparam>
        /// <param name="state">State.</param>
        /// <returns>Default.</returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            return default;
        }

        /// <summary>
        /// Checks if logLevel is enabled.
        /// </summary>
        /// <param name="logLevel">LogLevel.</param>
        /// <returns>Boolean representing whether logLevel is enabled.</returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= this.config.LogLevel;
        }

        /// <summary>
        /// Log event and include correlation vector if exists.
        /// </summary>
        /// <typeparam name="TState">State type.</typeparam>
        /// <param name="logLevel">Log level.</param>
        /// <param name="eventId">Event ID.</param>
        /// <param name="state">State.</param>
        /// <param name="exception">Exception.</param>
        /// <param name="formatter">Formatter.</param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!this.IsEnabled(logLevel))
            {
                return;
            }

            Dictionary<string, object> d = new ()
            {
                { "date", DateTime.UtcNow },
                { "logName", this.name },
                { "logLevel", logLevel.ToString() },
                { "eventId", eventId.Id },
                { "eventName", eventId.Name },
            };

            if (!string.IsNullOrEmpty(Zone))
            {
                d.Add("Zone", Zone);
            }

            if (!string.IsNullOrEmpty(Region))
            {
                d.Add("Region", Region);
            }

            // Adds custom item to Dictionary.
            if (this.logValues?.GetLogValues()?.Count > 0)
            {
                foreach (var kv in this.logValues.GetLogValues())
                {
                    d.Add(kv.Key, kv.Value);
                }
            }

            // convert state to list
            if (state is IReadOnlyList<KeyValuePair<string, object>> roList)
            {
                List<KeyValuePair<string, object>> list = roList.ToList();

                switch (list.Count)
                {
                    case 0:
                        break;
                    case 1:
                        // clean up name
                        list.Add(new KeyValuePair<string, object>("message", list[0].Value));
                        list.RemoveAt(0);
                        break;
                    default:
                        // remove formatting key-value
                        list.RemoveAt(list.Count - 1);
                        break;
                }

                HttpContext c = null;

                for (int i = list.Count - 1; i >= 0; i--)
                {
                    // get correlation vector from HttpContext.Items
                    if (c == null && list[i].Value is HttpContext)
                    {
                        c = list[i].Value as HttpContext;

                        if (c != null && c.Items != null)
                        {
                            CorrelationVector cv = CorrelationVectorExtensions.GetCorrelationVectorFromContext(c);

                            if (cv != null)
                            {
                                d.Add("CVector", cv.Value);
                            }
                        }
                    }
                    else
                    {
                        d.Add(list[i].Key.ToString(), list[i].Value == null ? string.Empty : list[i].Value.ToString());
                    }
                }
            }

            // add exception
            if (exception != null)
            {
                d.Add("Exception", exception.Message);
            }

            if (logLevel >= LogLevel.Error)
            {
                Console.ForegroundColor = ConsoleColor.Red;

                Console.Error.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(d));
            }
            else
            {
                Console.ForegroundColor = logLevel == LogLevel.Warning ? ConsoleColor.Yellow : Console.ForegroundColor;
                Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(d));
            }

            Console.ResetColor();

            // free the memory for GC
            d.Clear();
        }
    }
}
