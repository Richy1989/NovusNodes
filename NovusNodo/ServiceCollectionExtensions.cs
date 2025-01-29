using NovusNodo.Management;

namespace NovusNodo
{
    /// <summary>
    /// Extension methods for setting up NovusNodo services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the core components of NovusNodo to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
        public static IServiceCollection AddNovusNodeComponents(this IServiceCollection services)
        {
            // Register your services, components, etc., here.
            services.AddSingleton<CopyPasteCutManager>();
            services.AddSingleton<NovusUIManagement>();

            return services;
        }
    }

}
