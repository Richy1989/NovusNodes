using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace NovusNodoCore.NovusLogger
{
    public static class DebugWindowLoggerExtension
    {
        public static ILoggingBuilder AddNovusDebugWindowLogger(this ILoggingBuilder builder)
        {
            builder.AddConfiguration();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, DebugWindowLoggerProvider>());

            //LoggerProviderOptions.RegisterProviderOptions<DebugWindowLoggerConfiguration, DebugWindowLoggerProvider>(builder.Services);

            return builder;
        }
    }
}
