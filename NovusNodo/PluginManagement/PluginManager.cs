using McMaster.NETCore.Plugins;
using Microsoft.Extensions.FileProviders;
using NovusNodoPluginLibrary;

namespace NovusNodo.PluginManagement
{
    public static class PluginManager
    {
        public static List<StaticFileOptions> StaticFileOptions { get; } = new();
        public static string PluginPath { get; } = Path.Combine(Directory.GetCurrentDirectory(), "plugins");
        /// <summary>
        /// Adds plugin components to the service collection.
        /// </summary>
        /// <param name="services">The service collection to add the plugin components to.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddPluginComponents(this IServiceCollection services)
        {
            if (!Directory.Exists(PluginPath))
            {
                Directory.CreateDirectory(PluginPath);
            }

            foreach (var dir in Directory.GetDirectories(PluginPath))
            {
                var pluginFile = Path.Combine(dir, Path.GetFileName(dir) + ".dll");

                services.AddRazorPages().AddPluginFromAssemblyFile(pluginFile);
            }

            return services;
        }

        /// <summary>
        /// Loads plugins from the specified directories and adds them to the execution manager.
        /// </summary>
        /// <param name="services">The service collection to add the plugin components to.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddSimplePluginComponents(this IServiceCollection services)
        {
            var loaders = new List<PluginLoader>();

            foreach (var path in GetLibPaths())
            {
                string[] files;
                try
                {
                    files = Directory.GetFiles(path, "*.dll");
                }
                catch
                {
                    continue;
                }

                foreach (var file in files)
                {
                    var loader = PluginLoader.CreateFromAssemblyFile(
                    assemblyFile: file,
                    sharedTypes: [typeof(PluginBase), typeof(IPluginBase), typeof(ILogger)],
                    isUnloadable: false);

                    loaders.Add(loader);
                }
            }

            // Create an instance of plugin types
            foreach (var loader in loaders)
            {
                loader.LoadDefaultAssembly();
            }

            return services;
        }

        /// <summary>
        /// Gets the list of library paths to search for plugins.
        /// </summary>
        /// <returns>A list of library paths.</returns>
        private static List<string> GetLibPaths()
        {
            List<string> paths = new();
            string executingDir = Directory.GetCurrentDirectory();
            paths.Add(Path.Combine(executingDir, "../", "NovusNodoPlugins", "bin", "Debug", "net9.0"));
            return paths;
        }

        /// <summary>
        /// Loads static files of plugins into the application's wwwroot directory.
        /// </summary>
        public static IServiceCollection LoadStaticFilesOfPlugins(this IServiceCollection services)
        {
            var executingDir = Directory.GetCurrentDirectory();
            foreach (var pluginDirs in Directory.GetDirectories(PluginPath))
            {
                var staticFileDir = Directory.GetDirectories(pluginDirs, "wwwroot");

                if (staticFileDir.Length > 0)
                {
                    var staticFileContentDir = Path.Combine(staticFileDir[0], "_content");

                    foreach (var dir in Directory.GetDirectories(staticFileContentDir))
                    {
                        if (dir.Contains("MudBlazor"))
                        {
                            continue;
                        }

                        StaticFileOptions.Add(new StaticFileOptions
                        {
                            FileProvider = new PhysicalFileProvider(dir)
                        });

                        string PluginStaticFilesDirName = Path.GetFileName(Path.TrimEndingDirectorySeparator(dir));
                        string localwwwRootPath = Path.Combine(executingDir, "wwwroot", "_content", PluginStaticFilesDirName);
                        // If the plugin does not exist we copy the files
                        if (Directory.Exists(localwwwRootPath))
                        {
                            Directory.Delete(localwwwRootPath, true); // true indicates recursive deletion
                        }

                        //Directory.CreateDirectory(localwwwRootPath);
                        //foreach (string filePath in Directory.GetFiles(dir))
                        //{
                        //    string fileName = Path.GetFileName(Path.TrimEndingDirectorySeparator(filePath));
                        //    File.Copy(filePath, Path.Combine(localwwwRootPath, fileName), true); // Overwrite if exists
                        //}
                    }
                }
            }
            return services;
        }
    }
}