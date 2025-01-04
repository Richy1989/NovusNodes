using Microsoft.Extensions.DependencyInjection;
using NovusNodoCore.Managers;

namespace NovusNodoCore
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNovusCoreComponents(this IServiceCollection services)
        {
            // Register your services, components, etc., here.
            services.AddSingleton<ExecutionManager>();
            services.AddSingleton<NodeJSEnvironmentManager>();

            // Add other services or configurations as needed.

            return services;
        }
    }
}
