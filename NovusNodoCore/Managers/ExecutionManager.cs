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
        /// Event fired when the curve style is changed.
        /// </summary>
        public event Func<bool, Task> OnCurveStyleChanged;


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
            nodePage.PageName = nodePage.PageID;// "Nodes";
            NodePages.Add(nodePage.PageID, nodePage);
            OnPageChanged?.Invoke(PageAction.Added, nodePage.PageID);
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
            if (DebugLogChanged.Invoke != null)
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
            if (DebugLogChanged.Invoke != null)
                await DebugLogChanged.Invoke(message.Id, message).ConfigureAwait(false);
        }
    }
}
