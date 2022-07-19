// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LodeRunner.API.Middleware
{
    /// <summary>
    /// Custom TimeSpan Converter
    ///
    /// 00:00:00.123.
    /// </summary>
    public class TimeSpanConverter : JsonConverter<TimeSpan>
    {
        /// <summary>
        /// Reads a formatted time interval.
        /// </summary>
        /// <param name="reader">Utf8JsonReader.</param>
        /// <param name="typeToConvert">Type to convert.</param>
        /// <param name="options">Json serializer options.</param>
        /// <returns>Culturally specific, formatted time interval.</returns>
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return TimeSpan.Parse(reader.GetString(), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Writes a formatted timespan.
        /// </summary>
        /// <param name="writer">Utf8JsonWriter.</param>
        /// <param name="value">Time span.</param>
        /// <param name="options">Json serializer options.</param>
        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            // write timespan to milliseconds
            writer.WriteStringValue(value.ToString(@"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture));
        }
    }
}
