// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.CorrelationVector;
using Microsoft.Extensions.Logging;
using CorrelationVectorExtensions = LodeRunner.Core.Extensions.CorrelationVectorExtensions;

namespace LodeRunner.API.Middleware
{
    /// <summary>
    /// Application logger.
    /// </summary>
    public class NgsaLog
    {
        private static readonly JsonSerializerOptions Options = new ()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        /// <summary>
        /// Gets or sets LogLevel.
        /// </summary>
        public static LogLevel LogLevel { get; set; } = LogLevel.Information;

        /// <summary>
        /// Gets or sets Zone.
        /// </summary>
        public static string Zone { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets Region.
        /// </summary>
        public static string Region { get; set; } = string.Empty;

        /// <summary>
        /// Gets 400 log event.
        /// </summary>
        public static LogEventId LogEvent400 { get; } = new ((int)HttpStatusCode.BadRequest, HttpStatusCode.BadRequest.ToString());

        /// <summary>
        /// Gets 404 log event.
        /// </summary>
        public static LogEventId LogEvent404 { get; } = new ((int)HttpStatusCode.NotFound, HttpStatusCode.NotFound.ToString());

        /// <summary>
        /// Gets 500 log event.
        /// </summary>
        public static LogEventId LogEvent500 { get; } = new ((int)HttpStatusCode.InternalServerError, "Exception");

        /// <summary>
        /// Gets or sets Name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets ErrorMessage.
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets NotFoundError.
        /// </summary>
        public string NotFoundError { get; set; } = string.Empty;

        /// <summary>
        /// Log information message.
        /// </summary>
        /// <param name="method">method to log.</param>
        /// <param name="message">message to log.</param>
        /// <param name="context">http context.</param>
        /// <param name="dictionary">optional dictionary.</param>
        /// <returns>The Task.</returns>
        public async Task LogInformation(string method, string message, HttpContext context = null, Dictionary<string, object> dictionary = null)
        {
            if (LogLevel <= LogLevel.Information)
            {
                await WriteLog(LogLevel.Information, this.GetDictionary(method, message, LogLevel.Information, null, context, dictionary));
            }
        }

        /// <summary>
        /// Log warning.
        /// </summary>
        /// <param name="method">method to log.</param>
        /// <param name="message">message to log.</param>
        /// <param name="eventId">Event ID.</param>
        /// <param name="context">http context.</param>
        /// <param name="dictionary">optional dictionary.</param>
        /// <returns>The Task.</returns>
        public async Task LogWarning(string method, string message, LogEventId eventId = null, HttpContext context = null, Dictionary<string, object> dictionary = null)
        {
            if (LogLevel <= LogLevel.Warning)
            {
                await WriteLog(LogLevel.Warning, this.GetDictionary(method, message, LogLevel.Warning, eventId, context, dictionary));
            }
        }

        /// <summary>
        /// Log error.
        /// </summary>
        /// <param name="method">method to log.</param>
        /// <param name="message">message to log.</param>
        /// <param name="eventId">Event ID.</param>
        /// <param name="context">http context.</param>
        /// <param name="ex">exception.</param>
        /// <param name="dictionary">optional dictionary.</param>
        /// <returns>The Task.</returns>
        public async Task LogError(string method, string message, LogEventId eventId = null, HttpContext context = null, Exception ex = null, Dictionary<string, object> dictionary = null)
        {
            if (LogLevel <= LogLevel.Error)
            {
                Dictionary<string, object> d = this.GetDictionary(method, message, LogLevel.Error, eventId, context);

                // add exception
                if (ex != null)
                {
                    d.Add("ExceptionType", ex.GetType().FullName);
                    d.Add("ExceptionMessage", ex.Message);
                }

                // add dictionary
                if (dictionary != null && dictionary.Count > 0)
                {
                    foreach (KeyValuePair<string, object> kv in dictionary)
                    {
                        d.Add(kv.Key, kv.Value);
                    }
                }

                // log the error
                await WriteLog(LogLevel.Error, d);
            }
        }

        // write the log to console or console.error
        private static async Task WriteLog(LogLevel logLevel, Dictionary<string, object> data)
        {
            await Task.Run(() =>
            {
                Console.ForegroundColor = logLevel switch
                {
                    LogLevel.Error => ConsoleColor.Red,
                    LogLevel.Warning => ConsoleColor.Yellow,
                    _ => ConsoleColor.Green,
                };

                if (logLevel == LogLevel.Error)
                {
                    Console.Error.WriteLine(JsonSerializer.Serialize(data, Options));
                }
                else
                {
                    Console.WriteLine(JsonSerializer.Serialize(data, Options));
                }

                Console.ResetColor();
            });
        }

        // convert log to dictionary
        private Dictionary<string, object> GetDictionary(string method, string message, LogLevel logLevel, LogEventId eventId = null, HttpContext context = null, Dictionary<string, object> dictionary = null)
        {
            Dictionary<string, object> data = new ()
            {
                { "Date", DateTime.UtcNow },
                { "LogName", this.Name },
                { "Method", method },
                { "Message", message },
                { "LogLevel", logLevel.ToString() },
            };

            if (context != null && context.Items != null)
            {
                data.Add("Path", RequestLogger.GetPathAndQuerystring(context.Request));

                if (context.Items != null)
                {
                    CorrelationVector cv = CorrelationVectorExtensions.GetCorrelationVectorFromContext(context);

                    if (cv != null)
                    {
                        data.Add("CVector", cv.Value);
                    }
                }
            }

            // add LogEventId
            if (eventId != null && eventId.Id > 0)
            {
                data.Add("EventId", eventId.Id);
            }

            if (eventId != null && !string.IsNullOrWhiteSpace(eventId.Name))
            {
                data.Add("EventName", eventId.Name);
            }

            // add Zone and Region
            if (!string.IsNullOrEmpty(Zone))
            {
                data.Add("Zone", Zone);
            }

            if (!string.IsNullOrEmpty(Region))
            {
                data.Add("Region", Region);
            }

            // add dictionary
            if (dictionary != null && dictionary.Count > 0)
            {
                foreach (KeyValuePair<string, object> kv in dictionary)
                {
                    data.Add(kv.Key, kv.Value);
                }
            }

            return data;
        }
    }
}
