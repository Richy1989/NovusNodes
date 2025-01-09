using System.Reflection;
using System.Runtime.Loader;
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
        public List<Assembly> LoadedAssemblies { get; set; } = [];
        private ILogger<PluginLoader> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginLoader"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public PluginLoader(ILogger<PluginLoader> logger)
        {
            //Add Novus Core services
            this.logger = logger;
        }

        public void LoadUIPlugins()
        {
            string executingDir = Directory.GetCurrentDirectory();
            string path = Path.Combine(executingDir, "plugins");

            foreach (var pluginDirs in Directory.GetDirectories(path))
            {
                var staticFileDir = Directory.GetDirectories(pluginDirs, "wwwroot");
                foreach (var file in Directory.GetFiles(pluginDirs, "*.dll"))
                {
                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(file);
                    var list = assembly.GetExportedTypes().Select(s => (s.FullName ?? "", s.BaseType?.Name ?? "")).ToList();
                    
                    LoadedAssemblies.Add(assembly);
                }
                if (staticFileDir.Length > 0)
                {
                    var staticFileContentDir = Path.Combine(staticFileDir[0], "_content");

                    foreach (var dir in Directory.GetDirectories(staticFileContentDir))
                    {
                        if (dir.Contains("MudBlazor"))
                        {
                            continue;
                        }

                        string PluginStaticFilesDirName = Path.GetFileName(Path.TrimEndingDirectorySeparator(dir));
                        string localwwwRootPath = Path.Combine(executingDir, "wwwroot", "_content", PluginStaticFilesDirName);
                        //If the plugin does not exist we copy the files
                        if (Directory.Exists(localwwwRootPath))
                        {
                            Directory.Delete(localwwwRootPath, true); // true indicates recursive deletion
                        }

                        Directory.CreateDirectory(localwwwRootPath);
                        foreach (string filePath in Directory.GetFiles(dir))
                        {
                            string fileName = Path.GetFileName(Path.TrimEndingDirectorySeparator(filePath));
                            File.Copy(filePath, Path.Combine(localwwwRootPath, fileName), true); // Overwrite if exists
                        }
                    }
                }
            }
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
                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(file);

                    GetEmbeddedResourceContent(assembly);
                    //var assembly = Assembly.LoadFile(file);

                    LoadedAssemblies.Add(assembly);
                    Console.WriteLine($"Loaded Assembly Name: {assembly.GetName()}");
                }
            }
        }

        public static void GetEmbeddedResourceContent(Assembly assembly)
        {
            var stream = assembly.GetManifestResourceNames();



            //using var reader = new StreamReader(stream);
            //return reader.ReadToEnd();
        }

        public void RegisterPluginsAtExecutionManager(ExecutionManager executionManager)
        {
            foreach (var assembly in LoadedAssemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    //if (type.GetInterfaces().Contains(typeof(IPluginBase)))
                    if (type.BaseType == typeof(PluginBase))
                    {
                        var instance = Activator.CreateInstance(type);

                        if (instance == null)
                        {
                            continue;
                        }

                        PluginBase plugin = (PluginBase)instance;

                        executionManager.AvailablePlugins.Add(plugin.ID, plugin);
                        logger.LogInformation("Loaded plugin: {0}", plugin.Name);
                    }
                }
            }
        }

        //private static void getPackageByNameAndVersion(string packageID, string version)
        //{
        //    IPackageRepository repo =
        //            PackageRepositoryFactory.Default
        //                  .CreateRepository("https://packages.nuget.org/api/v2");

        //    string path = "C:/tmp_repo";
        //    PackageManager packageManager = new PackageManager(repo, path);
        //    Console.WriteLine("before dl pkg");
        //    packageManager.InstallPackage(packageID, SemanticVersion.Parse(version));

        //}

        /// <summary>
        /// Gets the list of library paths to search for plugins.
        /// </summary>
        /// <returns>A list of library paths.</returns>
        private List<string> GetLibPaths()
        {
            List<string> paths = new();
            string executingDir = Directory.GetCurrentDirectory();
            paths.Add(Path.Combine(executingDir, "../", "NovusNodoPlugins", "bin", "Debug", "net9.0"));
            //paths.Add(Path.Combine(executingDir, "../", "NovusNodoUIPlugins", "bin", "Debug", "net9.0"));
            return paths;
        }
    }
}
