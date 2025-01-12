using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NovusNodoCore.DebugNotification
{
    public class EventLogger : ILogger
    {
        public event Action<LogLevel, string, Exception> LogMessageReceived;

        private readonly string _categoryName;

        public EventLogger(string categoryName)
        {
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = formatter(state, exception);
            LogMessageReceived?.Invoke(logLevel, message, exception);
        }
    }
}