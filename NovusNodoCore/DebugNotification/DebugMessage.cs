﻿using System.Text.Json.Nodes;

namespace NovusNodoCore.DebugNotification
{
    /// <summary>
    /// Represents a debug message with various properties.
    /// </summary>
    public class DebugMessage
    {
        /// <summary>
        /// Gets or sets the unique identifier for the debug message.
        /// </summary>
        public string Id { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the type of the debug message.
        /// </summary>
        public string DebugType { get; set; }

        /// <summary>
        /// Gets or sets the tag associated with the debug message.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the debug message was created.
        /// </summary>
        public DateTime DateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the exception associated with the debug message, if any.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Gets or sets the message content in JSON format.
        /// </summary>
        public JsonObject Message { get; set; }

        /// <summary>
        /// Gets or sets the sender of the debug message.
        /// </summary>
        public string Sender { get; set; }

        /// <summary>
        /// Gets or sets the error message, if any.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
