using Microsoft.Extensions.Logging;

namespace NovusNodoCore.DebugNotification
{
    public class NovusLoggerProvider : ILoggerProvider
    {
        public event Action<LogLevel, string, Exception> LogMessageReceived;

        public ILogger CreateLogger(string categoryName)
        {
            var logger = new EventLogger(categoryName);
            logger.LogMessageReceived += (logLevel, message, exception) => LogMessageReceived?.Invoke(logLevel, message, exception);
            return logger;
        }

        public void Dispose() { }
    }
}