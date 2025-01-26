using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using NovusNodoCore.DebugNotification;
using NovusNodoCore.Enumerations;
using NovusNodoCore.NovusLogger;
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

        /// <summary>
        /// The logger instance for logging errors and information.
        /// </summary>
        private readonly ILogger<ExecutionManager> logger;

        /// <summary>
        /// Event fired when DebugLog is updated.
        /// </summary>
        public event Func<string, DebugMessage, Task> DebugLogChanged;

        /// <summary>
        /// Event fired when the project is changed, to trigger a save file.
        /// </summary>
        public event Func<string, Task> ProjectChanged;

        public event Func<Task> OnManualSaveTrigger;

        public event Func<Task> OnProjectSaved;

        public bool ProjectDataSynced { get; set; } = true;

        public bool IsAutoSaveEnabled { get; set; } = true;

        /// <summary>
        /// Event fired when the curve style is changed.
        /// </summary>
        public event Func<bool, Task> OnCurveStyleChanged;

        /// <summary>
        /// Event fired when the page is changed.
        /// </summary>
        public event Func<PageAction, string, Task> OnPageChanged;

        /// <summary>
        /// Gets or sets a value indicating whether execution is allowed.
        /// </summary>
        public bool IsExecutionAllowed { get; set; } = true;

        private bool useBezierCurve = true;
        /// <summary>
        /// Gets or sets a value indicating whether to use a Bezier curve.
        /// </summary>
        public bool UseBezierCurve
        {
            get
            {
                return useBezierCurve;
            }
            set
            {
                this.useBezierCurve = value;
                OnCurveStyleChanged(value);
            }
        }

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
        public IDictionary<string, DebugMessage> DebugLog { get; set; } = new Dictionary<string, DebugMessage>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionManager"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="nodeJSEnvironmentManager">The NodeJS environment manager.</param>
        public ExecutionManager(ILogger<ExecutionManager> logger, IServiceProvider serviceProvider, NodeJSEnvironmentManager nodeJSEnvironmentManager)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
            NodeJSEnvironmentManager = nodeJSEnvironmentManager;
            NodeJSEnvironmentManager.Initialize();

            DebugWindowLogger.NewDebugLog += ExecutionManager_NewDebugLog;

            // Subscribe to the project changed event of this class to update the ProjectDataSynced property.
            this.ProjectChanged += ExecutionManager_ProjectChanged;
        }

        /// <summary>
        /// Handles the event when the project is changed.
        /// </summary>
        /// <param name="arg">The argument indicating the project change.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ExecutionManager_ProjectChanged(string arg)
        {
            ProjectDataSynced = false;
            await Task.CompletedTask.ConfigureAwait(false);
        }

        public async Task AllProjectDataSynced()
        {
            ProjectDataSynced = true;
            if (OnProjectSaved != null)
                await OnProjectSaved.Invoke().ConfigureAwait(false);
        }

        public async Task ManualSaveTrigger()
        {
            if (OnManualSaveTrigger != null)
                await OnManualSaveTrigger.Invoke().ConfigureAwait(false);
        }

        /// <summary>
        /// Adds a new tab and returns the created <see cref="NodePageManager"/>.
        /// </summary>
        /// <param name="id">The ID of the new tab. If null, a new GUID will be generated.</param>
        /// <param name="isStartup">Indicates whether the tab is a startup tab.</param>
        /// <returns>The created <see cref="NodePageManager"/>.</returns>
        public async Task<NodePageManager> AddNewTab(string id = null, bool isStartup = false)
        {
            var nodePage = (NodePageManager)serviceProvider.GetService(typeof(NodePageManager));

            //Add event handler to get notified when the page data is changed, in order to save it. 
            nodePage.OnPageDataChanged += NodePage_OnPageDataChanged;

            nodePage.DebugLogChanged = OnDebugLogUpdated;
            nodePage.PageID = id ?? Guid.NewGuid().ToString();
            nodePage.PageName = "Nodes";
            NodePages.Add(nodePage.PageID, nodePage);
            OnPageChanged?.Invoke(PageAction.Added, nodePage.PageID);

            if (ProjectChanged != null && !isStartup)
                await ProjectChanged.Invoke(nodePage.PageID).ConfigureAwait(false);

            return nodePage;
        }

        /// <summary>
        /// Handles the event when the page data is changed.
        /// </summary>
        /// <param name="pageId">The ID of the page that was changed.</param>
        public async Task NodePage_OnPageDataChanged(string pageId)
        {
            if (ProjectChanged != null)
                await ProjectChanged.Invoke(pageId).ConfigureAwait(false);
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

            OnPageChanged?.Invoke(PageAction.Removed, nodePage.PageID);

            foreach (var node in nodePage.AvailableNodes)
            {
                node.Value.OutputPorts.Clear();
                node.Value.OutputPorts = null;
                node.Value.InputPort = null;
            }
        }

        /// <summary>
        /// Handles the event when a new debug log is created.
        /// </summary>
        /// <param name="arg">The debug message argument.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ExecutionManager_NewDebugLog(DebugMessage arg)
        {
            await OnDebugLogUpdated(arg).ConfigureAwait(false);
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

                    if (baseAttribute == null)
                    {
                        continue;
                    }
                    if (baseAttribute.Id == "BASE")
                    {
                        logger.LogError("Cannot load Base class of plugin, or ID was not set");
                        throw new Exception("Cannot load Base class of plugin, or ID was not set");
                    }
                    if (!Guid.TryParse(baseAttribute.Id, out _))
                    {
                        logger.LogError("Plugin ID must be a valid GUID");
                        throw new Exception("Plugin ID must be a valid GUID");
                    }

                    // Find the configuration type for the plugin if available
                    var pluginConfigType = FindConfigType(type);
                    baseAttribute.PluginConfigType = pluginConfigType;
                    baseAttribute.AssemblyName = assembly.GetName().Name;

                    AvailablePlugins.Add(baseAttribute.Id, (type, baseAttribute));
                }
            }
        }

        /// <summary>
        /// Finds the configuration type for the specified plugin type.
        /// </summary>
        /// <param name="pluginType">The plugin type.</param>
        /// <returns>The configuration type for the plugin.</returns>
        public Type FindConfigType(Type pluginType)
        {
            string name = pluginType.Name.Replace("Plugin", "Config");
            return pluginType.Assembly.GetTypes().FirstOrDefault(x => x.Name == name);

        }

        /// <summary>
        /// Invokes the DebugLogChanged event.
        /// </summary>
        /// <param name="senderId">The ID of the sender.</param>
        /// <param name="inputMessage">The input message in JSON format.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task OnDebugLogUpdated(string senderId, JsonObject inputMessage)
        {
            DebugMessage debugMessage = new()
            {
                DebugType = "Debug",
                Tag = "Debug",
                Message = inputMessage,
                Sender = senderId
            };

            DebugLog.Add(debugMessage.Id, debugMessage);
            if (DebugLogChanged != null)
                await DebugLogChanged.Invoke(debugMessage.Id, debugMessage).ConfigureAwait(false);
        }

        /// <summary>
        /// Invokes the DebugLogChanged event.
        /// </summary>
        /// <param name="message">The debug log message.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task OnDebugLogUpdated(DebugMessage message)
        {
            DebugLog.Add(message.Id, message);
            if (DebugLogChanged != null)
                await DebugLogChanged.Invoke(message.Id, message).ConfigureAwait(false);
        }
    }
}
