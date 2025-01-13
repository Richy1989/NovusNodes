using Microsoft.Extensions.Logging;
using NovusNodoCore.DebugNotification;
using NovusNodoCore.Managers;

namespace NovusNodoCore.NovusLogger
{
    public sealed class DebugWindowLoggerConfiguration
    {
        public int EventId { get; set; }
    }

    public sealed class DebugWindowLogger(string name) : ILogger
    {
        public static event Func<DebugMessage, Task> NewDebugLog;
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => default!;

        public bool IsEnabled(LogLevel logLevel) => true;

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

            //ColorConsoleLoggerConfiguration config = getCurrentConfig();
            //if (config.EventId == 0 || config.EventId == eventId.Id)
            //{
            //    ConsoleColor originalColor = Console.ForegroundColor;

            //    Console.ForegroundColor = config.LogLevelToColorMap[logLevel];
            //    Console.WriteLine($"[{eventId.Id,2}: {logLevel,-12}]");

            //    Console.ForegroundColor = originalColor;
            //    Console.Write($"     {name} - ");

            //    Console.ForegroundColor = config.LogLevelToColorMap[logLevel];
            //    Console.Write($"{formatter(state, exception)}");

            //    Console.ForegroundColor = originalColor;
            //    Console.WriteLine();
            //}
        }
    }
}