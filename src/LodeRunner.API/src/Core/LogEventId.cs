// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace LodeRunner.API.Middleware
{
    /// <summary>
    /// Nullable EventId class for logging.
    /// </summary>
    public class LogEventId
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogEventId"/> class.
        /// </summary>
        /// <param name="eventId">eventId represented by StatusCode.</param>
        /// <param name="name">name of event.</param>
        public LogEventId(int eventId, string name)
        {
            Id = eventId;
            Name = name;
        }

        /// <summary>
        /// Gets or sets Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets event name.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
