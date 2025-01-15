using Microsoft.Extensions.Logging;
using NovusNodoCore.DebugNotification;

namespace NovusNodoCore.NovusLogger
{
    /// <summary>
    /// Configuration settings for the DebugWindowLogger.
    /// </summary>
    public sealed class DebugWindowLoggerConfiguration
    {
        /// <summary>
        /// Gets or sets the event ID.
        /// </summary>
        public int EventId { get; set; }
    }

    /// <summary>
    /// A logger that sends log messages to a debug window.
    /// </summary>
    /// <param name="name">The name of the logger.</param>
    public sealed class DebugWindowLogger(string name) : ILogger
    {
        /// <summary>
        /// Event triggered when a new debug log message is created.
        /// </summary>
        public static event Func<DebugMessage, Task> NewDebugLog;

        /// <summary>
        /// Begins a logical operation scope.
        /// </summary>
        /// <typeparam name="TState">The type of the state.</typeparam>
        /// <param name="state">The identifier for the scope.</param>
        /// <returns>A disposable object that ends the logical operation scope on dispose.</returns>
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => default!;

        /// <summary>
        /// Checks if the given log level is enabled.
        /// </summary>
        /// <param name="logLevel">The log level to check.</param>
        /// <returns>True if the log level is enabled; otherwise, false.</returns>
        public bool IsEnabled(LogLevel logLevel) => true;

        /// <summary>
        /// Logs a message at the specified log level.
        /// </summary>
        /// <typeparam name="TState">The type of the state.</typeparam>
        /// <param name="logLevel">The log level.</param>
        /// <param name="eventId">The event ID.</param>
        /// <param name="state">The state to log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="formatter">The function to create a log message from the state and exception.</param>
        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (logLevel == LogLevel.Error)
            {
                DebugMessage message = new DebugMessage()
                {
                    Tag = "Error",
                    Exception = exception,
                    Sender = name,
                    ErrorMessage = formatter(state, exception)
                };

                NewDebugLog?.Invoke(message);
            }
        }
    }
}