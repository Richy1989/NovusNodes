using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using NovusNodoCore.NodeDefinition;
using NovusNodoPluginLibrary;

namespace NovusNodoCore.Managers
{
    public class NodePageManager
    {
        public string PageID { get; set; }

        public string PageName { get; set; }

        /// <summary>
        /// The NodeJS environment manager.
        /// </summary>
        private readonly NodeJSEnvironmentManager NodeJSEnvironmentManager;

        /// <summary>
        /// Event fired when DebugLog is updated.
        /// </summary>
        public Func<string, JsonObject, Task> DebugLogChanged;

        /// <summary>
        /// The logger factory.
        /// </summary>
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// The cancellation Token source used to cancel operations.
        /// </summary>
        public CancellationTokenSource CancellationTokenSource { get; }

        /// <summary>
        /// The cancellation Token used to observe cancellation requests.
        /// </summary>
        public CancellationToken Token { get; }

        /// <summary>
        /// Event fired when AvailableNodes is updated.
        /// </summary>
        public event Func<NodeBase, Task> AvailableNodesUpdated;

        /// <summary>
        /// Gets or sets the available nodes
        /// </summary>
        public IDictionary<string, NodeBase> AvailableNodes { get; set; } = new Dictionary<string, NodeBase>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionManager"/> class.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="nodeJSEnvironmentManager">The NodeJS environment manager.</param>
        public NodePageManager(ILoggerFactory loggerFactory, NodeJSEnvironmentManager nodeJSEnvironmentManager)
        {
            CancellationTokenSource = new CancellationTokenSource();
            Token = CancellationTokenSource.Token;

            this.loggerFactory = loggerFactory;

            NodeJSEnvironmentManager = nodeJSEnvironmentManager;
        }

        /// <summary>
        /// Creates a new node from the specified plugin.
        /// </summary>
        /// <param name="pluginBase">The plugin base to create the node from.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task<NodeBase> CreateNode(Type pluginBase, PluginIdAttribute attribute)
        {
            var instance = Activator.CreateInstance(pluginBase);

            if (instance == null)
            {
                return null;
            }

            var plugin = (IPluginBase)instance;

            if (plugin != null)
            {
                Logger<INodeBase> logger = (Logger<INodeBase>)loggerFactory.CreateLogger<INodeBase>();
                NodeBase node = new(plugin, attribute,logger, NodeJSEnvironmentManager, DebugLogChanged, Token);
                AvailableNodes.Add(node.Id, node);
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
        private async Task OnAvailableNodesUpdated(NodeBase nodeBase)
        {
            if (AvailableNodesUpdated == null)
            {
                await Task.CompletedTask;
            }
            await AvailableNodesUpdated.Invoke(nodeBase).ConfigureAwait(false);
        }
    }
}
