using Microsoft.Extensions.DependencyInjection;
using NovusNodoCore.Managers;
using NovusNodoCore.Tools;

namespace NovusNodoCore
{
    /// <summary>
    /// Extension methods for setting up NovusNodoCore services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the core components of NovusNodoCore to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
        public static IServiceCollection AddNovusCoreComponents(this IServiceCollection services)
        {
            // Register your services, components, etc., here.
            services.AddSingleton<ExecutionManager>();
            services.AddSingleton<NodeJSEnvironmentManager>();
            services.AddSingleton<LoadSaveManager>();
            services.AddTransient<NodePageManager>();
            services.AddTransient<NovusModelCreator>();

            return services;
        }
    }

}
