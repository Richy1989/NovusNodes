using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using NovusNodoPluginLibrary;

namespace NovusNodoCore.Managers
{
    /// <summary>
    /// Manages the execution of nodes and plugins.
    /// </summary>
    public class ExecutionManager
    {
        /// <summary>
        /// The NodeJS environment manager.
        /// </summary>
        private readonly NodeJSEnvironmentManager NodeJSEnvironmentManager;

        /// <summary>
        /// The service provider.
        /// </summary>
        private readonly IServiceProvider serviceProvider;

        private readonly ILogger logger;

        /// <summary>
        /// Event fired when DebugLog is updated.
        /// </summary>
        public event Func<(string, JsonObject), Task> DebugLogChanged;

        /// <summary>
        /// Gets or sets the available plugins.
        /// </summary>
        public IDictionary<string, (Type, NovusPluginAttribute)> AvailablePlugins { get; set; } = new Dictionary<string, (Type, NovusPluginAttribute)>();

        /// <summary>
        /// Gets or sets the available nodes per Tab page.
        /// </summary>
        public IDictionary<string, NodePageManager> NodePages { get; set; } = new Dictionary<string, NodePageManager>();

        /// <summary>
        /// Gets or sets the debug log.
        /// </summary>
        public IDictionary<string, JsonObject> DebugLog { get; set; } = new Dictionary<string, JsonObject>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionManager"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="pluginLoader">The plugin loader.</param>
        /// <param name="nodeJSEnvironmentManager">The NodeJS environment manager.</param>
        public ExecutionManager(ILogger<ExecutionManager> logger, IServiceProvider serviceProvider, NodeJSEnvironmentManager nodeJSEnvironmentManager)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;

            NodeJSEnvironmentManager = nodeJSEnvironmentManager;
            NodeJSEnvironmentManager.Initialize();
        }

        /// <summary>
        /// Initializes the execution manager by loading available plugins and saved data.
        /// </summary>
        public void Initialize()
        {
            // Get all loaded assemblies in the current application domain
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                // Get all types in the assembly
                var types = assembly.GetTypes();

                // Find types that are subclasses of PluginBase
                var derivedTypes = types.Where(t => t.IsSubclassOf(typeof(PluginBase)) && !t.IsAbstract);

                foreach (var type in derivedTypes)
                {

                    NovusPluginAttribute baseAttribute = (NovusPluginAttribute)Attribute.GetCustomAttribute(type, typeof(NovusPluginAttribute));

                    if(baseAttribute == null)
                    {
                        continue;
                    }
                    if (baseAttribute.Id == "BASE")
                    {
                        logger.LogError("Cannot load Base class of plugin, or ID was not set");
                        throw new Exception("Cannot load Base class of plugin, or ID was not set");
                    }
                    if(!Guid.TryParse(baseAttribute.Id, out _))
                    {
                        logger.LogError("Plugin ID must be a valid GUID");
                        throw new Exception("Plugin ID must be a valid GUID");
                    }

                    //var instance = Activator.CreateInstance(type);
                    //if (instance == null)
                    //{
                    //    continue;
                    //}
                    //PluginBase plugin = (PluginBase)instance;

                    AvailablePlugins.Add(baseAttribute.Id, (type, baseAttribute));
                }
            }

            LoadSavedData();
        }

        /// <summary>
        /// Loads saved data and adds a new tab.
        /// </summary>
        public void LoadSavedData()
        {
            AddNewTab();
        }

        /// <summary>
        /// Adds a new tab and returns the created <see cref="NodePageManager"/>.
        /// </summary>
        /// <returns>The created <see cref="NodePageManager"/>.</returns>
        public NodePageManager AddNewTab()
        {
            var nodePage = (NodePageManager)serviceProvider.GetService(typeof(NodePageManager));
            nodePage.DebugLogChanged = OnDebugLogUpdated;
            nodePage.PageID = Guid.NewGuid().ToString();
            nodePage.PageName = "Nodes";
            NodePages.Add(nodePage.PageID, nodePage);
            return nodePage;
        }

        /// <summary>
        /// Removes the tab with the specified page ID.
        /// </summary>
        /// <param name="pageID">The ID of the page to remove.</param>
        public void RemoveTab(string pageID)
        {
            var nodePage = NodePages[pageID];
            nodePage.CancellationTokenSource.Cancel();
            NodePages.Remove(pageID);

            foreach (var node in nodePage.AvailableNodes)
            {
                node.Value.OutputPorts.Clear();
                node.Value.OutputPorts = null;
                node.Value.InputPort = null;
            }
        }

        /// <summary>
        /// Invokes the DebugLogChanged event.
        /// </summary>
        /// <param name="id">The ID of the debug log entry.</param>
        /// <param name="message">The debug log message.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task OnDebugLogUpdated(string id, JsonObject message)
        {
            string dID = Guid.NewGuid().ToString();
            DebugLog.Add(dID, message);
            if (DebugLogChanged.Invoke != null)
                await DebugLogChanged.Invoke((dID, message)).ConfigureAwait(false);
        }
    }
}
