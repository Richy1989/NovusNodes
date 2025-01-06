using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using NovusNodoCore.NodeDefinition;
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
        /// The logger factory.
        /// </summary>
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// The cancellation token source used to cancel operations.
        /// </summary>
        private readonly CancellationTokenSource cts;

        /// <summary>
        /// The cancellation token used to observe cancellation requests.
        /// </summary>
        private readonly CancellationToken token;

        /// <summary>
        /// The plugin loader responsible for loading plugins.
        /// </summary>
        private readonly PluginLoader pluginLoader;

        /// <summary>
        /// Event fired when AvailableNodes is updated.
        /// </summary>
        public event Func<INodeBase, Task> AvailableNodesUpdated;

        /// <summary>
        /// Event fired when DebugLog is updated.
        /// </summary>
        public event Func<(string, JsonObject), Task> DebugLogChanged;

        /// <summary>
        /// Gets or sets the available plugins.
        /// </summary>
        public IDictionary<string, IPluginBase> AvailablePlugins { get; set; } = new Dictionary<string, IPluginBase>();

        /// <summary>
        /// Gets or sets the available nodes.
        /// </summary>
        public IDictionary<string, INodeBase> AvailableNodes { get; set; } = new Dictionary<string, INodeBase>();

        /// <summary>
        /// Gets or sets the debug log.
        /// </summary>
        public IDictionary<string, JsonObject> DebugLog { get; set; } = new Dictionary<string, JsonObject>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionManager"/> class.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="nodeJSEnvironmentManager">The NodeJS environment manager.</param>
        public ExecutionManager(ILoggerFactory loggerFactory, NodeJSEnvironmentManager nodeJSEnvironmentManager)
        {
            this.loggerFactory = loggerFactory;
            cts = new CancellationTokenSource();
            token = cts.Token;
            pluginLoader = new PluginLoader(this);

            this.NodeJSEnvironmentManager = nodeJSEnvironmentManager;
            NodeJSEnvironmentManager.Initialize();
        }

        /// <summary>
        /// Initializes the execution manager by loading plugins and creating test nodes.
        /// </summary>
        public void Initialize()
        {
            pluginLoader.LoadPlugins();
        }

        /// <summary>
        /// Creates a new node from the specified plugin.
        /// </summary>
        /// <param name="pluginBase">The plugin base to create the node from.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task<NodeBase> CreateNode(IPluginBase pluginBase)
        {
            var instance = Activator.CreateInstance(pluginBase.GetType());

            if (instance == null)
            {
                return null;
            }

            var plugin = (IPluginBase)instance;

            if (plugin != null)
            {
                var logger = loggerFactory.CreateLogger(typeof(NodeBase));
                NodeBase node = new(plugin, logger, NodeJSEnvironmentManager, OnDebugLogUpdated, token);
                AvailableNodes.Add(node.ID, node);
                await OnAvailableNodesUpdated(node).ConfigureAwait(false);
                return node;
            }
            return null;
        }

        /// <summary>
        /// Creates a new connection between two nodes.
        /// </summary>
        /// <param name="sourceId">The ID of the source node.</param>
        /// <param name="sourcePortId">The ID of the source port.</param>
        /// <param name="targetId">The ID of the target node.</param>
        /// <param name="targetPortId">The ID of the target port.</param>
        public void NewConnection(string sourceId, string sourcePortId, string targetId, string targetPortId)
        {
            if (AvailableNodes.TryGetValue(sourceId, out var sourceNode) && AvailableNodes.TryGetValue(targetId, out var targetNode))
            {
                sourceNode.OutputPorts[sourcePortId].AddConnection(targetPortId, targetNode);
            }
        }

        /// <summary>
        /// Removes the connection between the specified source and target nodes.
        /// </summary>
        /// <param name="sourceId">The ID of the source node.</param>
        /// <param name="sourcePortId">The ID of the source port.</param>
        /// <param name="targetId">The ID of the target node.</param>
        /// <param name="targetPortId">The ID of the target port.</param>
        public void RemoveConnection(string sourceId, string sourcePortId, string targetId, string targetPortId)
        {
            if (AvailableNodes.TryGetValue(sourceId, out var sourceNode))
            {
                sourceNode.OutputPorts[sourcePortId].RemoveConnection(targetPortId);
            }
        }

        /// <summary>
        /// Removes the specified node and all its connections.
        /// </summary>
        /// <param name="id">The ID of the node to remove.</param>
        public void ElementRemoved(string id)
        {
            if (AvailableNodes.TryGetValue(id, out var node))
            {
                AvailableNodes.Remove(id);

                // Remove connections from output ports
                foreach (var outputPort in node.OutputPorts)
                {
                    outputPort.Value.RemoveAllConnections();
                }

                // Remove connections from input port
                node.InputPort.RemoveAllConnections();
            }
        }

        /// <summary>
        /// Invokes the AvailableNodesUpdated event.
        /// </summary>
        /// <param name="nodeBase">The node that was updated.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task OnAvailableNodesUpdated(INodeBase nodeBase)
        {
            if (AvailableNodesUpdated == null)
            {
                await Task.CompletedTask;
            }
            await AvailableNodesUpdated.Invoke(nodeBase).ConfigureAwait(false);
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
            await DebugLogChanged.Invoke((dID, message)).ConfigureAwait(false);
        }
    }
}
