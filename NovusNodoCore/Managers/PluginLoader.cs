using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using NovusNodoPluginLibrary;

namespace NovusNodoCore.Managers
{
    /// <summary>
    /// Responsible for loading plugins from specified directories.
    /// </summary>
    public class PluginLoader
    {
        public static List<Assembly> loadedAssemblies = [];
        private ExecutionManager executionManager;
        private ILogger<PluginLoader> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginLoader"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public PluginLoader(ILogger<PluginLoader> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Initializes the plugin loader with the specified execution manager.
        /// </summary>
        /// <param name="executionManager">The execution manager instance.</param>
        public void Initialize(ExecutionManager executionManager)
        {
            this.executionManager = executionManager;
        }

        /// <summary>
        /// Loads plugins from the specified directories and adds them to the execution manager.
        /// </summary>
        public void LoadPlugins()
        {
            foreach (var path in GetLibPaths())
            {
                string[] files;
                try
                {
                    files = Directory.GetFiles(path, "*.dll");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error loading plugins from path: {0}", path);
                    continue;
                }

                foreach (var file in files)
                {
                    Console.WriteLine($"Loaded File Name: {file}");
                    var assembly = Assembly.LoadFile(file);
                    loadedAssemblies.Add(assembly);
                    Console.WriteLine($"Loaded Assembly Name: {assembly.GetName()}");
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        if (type.GetInterfaces().Contains(typeof(IPluginBase)))
                        {
                            var instance = Activator.CreateInstance(type);

                            if (instance == null)
                            {
                                continue;
                            }

                            IPluginBase plugin = (IPluginBase)instance;

                            executionManager.AvailablePlugins.Add(plugin.ID, plugin);
                            logger.LogInformation("Loaded plugin: {0}", plugin.Name);

                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the list of library paths to search for plugins.
        /// </summary>
        /// <returns>A list of library paths.</returns>
        private List<string> GetLibPaths()
        {
            List<string> paths = new();
            string executingDir = Directory.GetCurrentDirectory();
            paths.Add(Path.Combine(executingDir, "../", "NovusNodoPlugins", "bin", "Debug", "net9.0"));
            paths.Add(Path.Combine(executingDir, "../", "NovusNodoUIPlugins", "bin", "Debug", "net9.0"));
            return paths;
        }
    }
}
