using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using NovusNodoCore.NodeDefinition;
using NovusNodoPluginLibrary;

namespace NovusNodoCore.Managers
{
    internal class PluginLoader(ExecutionManager executionManager)
    {
        private readonly ExecutionManager executionManager = executionManager;
        private readonly List<string> paths = [];

        public void LoadPlugins()
        {
            paths.Add("C:\\Users\\richy\\SoftwarewDevelopment\\NodiAutomati\\NovusNodo\\NovusNodoPlugins\\bin\\Debug\\net8.0");
            paths.Add("C:\\Users\\richy\\SoftwarewDevelopment\\NodiAutomati\\NovusNodo\\NovusNodoUIPlugins\\bin\\Debug\\net8.0");

            foreach (var path in paths)
            {

                var files = Directory.GetFiles(path, "*.dll");

                foreach (var file in files)
                {
                    if (file.EndsWith("Plugins.dll"))
                    {
                        var assembly = Assembly.LoadFile(file);
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
                            }
                        }
                    }
                }
            }
        }
    }
}
