using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using NovusNodoCore.DebugNotification;
using NovusNodoCore.Enumerations;
using NovusNodoCore.Extensions;
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
        public event Func<Task> OnProjectChanged;

        /// <summary>
        /// Event fired when a manual save is triggered.
        /// </summary>
        public event Func<Task> OnManualSaveTrigger;

        /// <summary>
        /// Event fired when the project is saved.
        /// </summary>
        public event Func<Task> OnProjectSaved;

        /// <summary>
        /// Event fired when the curve style is changed.
        /// </summary>
        public event Func<bool, Task> OnCurveStyleChanged;

        /// <summary>
        /// Event fired when the page is changed.
        /// </summary>
        public event Func<PageAction, string, Task> OnPagesModified;

        /// <summary>
        /// Gets or sets a value indicating whether the project data is synced.
        /// </summary>
        public bool ProjectDataSynced { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether auto-save is enabled.
        /// </summary>
        public bool IsAutoSaveEnabled { get; set; } = true;

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
                //OnCurveStyleChanged(value);
                //Fire and forget this event
                _ = RaiseCurveStyleAsync(value).ConfigureAwait(false);
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
            this.OnProjectChanged += ExecutionManager_ProjectChanged;
        }

        /// <summary>
        /// Handles the event when the project is changed.
        /// </summary>
        /// <param name="arg">The argument indicating the project change.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ExecutionManager_ProjectChanged()
        {
            ProjectDataSynced = false;
            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the project data as synced and invokes the OnProjectSaved event.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AllProjectDataSynced()
        {
            ProjectDataSynced = true;
            await OnProjectSaved.RaiseAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Triggers a manual save and invokes the OnManualSaveTrigger event.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ManualSaveTrigger()
        {
            await OnManualSaveTrigger.RaiseAsync().ConfigureAwait(false);
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
            nodePage.OnPageDataChanged += RaiseProjectChangedAsync;

            nodePage.DebugLogChanged = OnDebugLogUpdated;

            nodePage.PageID = id ?? Guid.NewGuid().ToString();
            nodePage.PageName = "Nodes";
            NodePages.Add(nodePage.PageID, nodePage);

            await RaisePagesModifiedAsync(PageAction.Added, nodePage.PageID).ConfigureAwait(false);


            if (!isStartup)
                await OnProjectChanged.RaiseAsync().ConfigureAwait(false);

            return nodePage;
        }

        ///// <summary>
        ///// Handles the event when the page data is changed.
        ///// </summary>
        ///// <param name="pageId">The ID of the page that was changed.</param>
        //private async Task NodePage_OnPageDataChanged()
        //{
        //    if (OnProjectChanged != null)
        //        await OnProjectChanged.Invoke().ConfigureAwait(false);
        //}

        /// <summary>
        /// Removes the tab with the specified page ID.
        /// </summary>
        /// <param name="pageID">The ID of the page to remove.</param>
        public async Task RemoveTab(string pageID)
        {
            var nodePage = NodePages[pageID];
            nodePage.CancellationTokenSource.Cancel();
            NodePages.Remove(pageID);

            //OnPagesModified?.Invoke(PageAction.Removed, nodePage.PageID);
            await RaisePagesModifiedAsync(PageAction.Removed, nodePage.PageID).ConfigureAwait(false);

            foreach (var node in nodePage.AvailableNodes)
            {
                node.Value.OutputPorts.Clear();
                node.Value.OutputPorts = null;
                node.Value.InputPort = null;
            }

            await OnProjectChanged.RaiseAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the event when a new debug log is created.
        /// </summary>
        /// <param name="arg">The debug message argument.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ExecutionManager_NewDebugLog(DebugMessage arg)
        {
            await RaiseDebugLogUpdatedAsync(arg).ConfigureAwait(false);
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

            await RaiseDebugLogUpdatedAsync(debugMessage).ConfigureAwait(false);
        }

        /// <summary>
        /// Raises the DebugLogUpdated event asynchronously.
        /// </summary>
        /// <param name="message">The debug message to raise the event with.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RaiseDebugLogUpdatedAsync(DebugMessage message)
        {
            var handlers = DebugLogChanged?.GetInvocationList(); // Get all subscribers
            if (handlers == null) return;

            foreach (var handler in handlers.Cast<Func<string, DebugMessage, Task>>())
            {
                await handler(message.Id, message).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Raises the CurveStyleChanged event asynchronously.
        /// </summary>
        /// <param name="isBezierCurve">Indicates whether the curve style is Bezier.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RaiseCurveStyleAsync(bool isBezierCurve)
        {
            var handlers = OnCurveStyleChanged?.GetInvocationList(); // Get all subscribers
            if (handlers == null) return;

            foreach (var handler in handlers.Cast<Func<bool, Task>>())
            {
                await handler(isBezierCurve).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Raises the PagesModified event asynchronously.
        /// </summary>
        /// <param name="pageAction">The action performed on the page.</param>
        /// <param name="pageId">The ID of the page.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RaisePagesModifiedAsync(PageAction pageAction, string pageId)
        {
            var handlers = OnPagesModified?.GetInvocationList(); // Get all subscribers
            if (handlers == null) return;

            foreach (var handler in handlers.Cast<Func<PageAction, string, Task>>())
            {
                await handler(pageAction, pageId).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Raises the ProjectChanged event asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RaiseProjectChangedAsync()
        {
            await OnProjectChanged.RaiseAsync().ConfigureAwait(false);
        }
    }
}
