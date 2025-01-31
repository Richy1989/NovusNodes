using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using NovusNodoCore.NodeDefinition;
using NovusNodoPluginLibrary;

namespace NovusNodoCore.Managers
{
    public class NodePageManager
    {
        /// <summary>
        /// Gets or sets the Page ID.
        /// </summary>
        public string PageID { get; set; }

        /// <summary>
        /// Gets or sets the Page Name.
        /// </summary>
        public string PageName { get; set; }

        /// <summary>
        /// Event fired when the project is changed, to trigger a save file.
        /// </summary>
        public event Func<Task> OnPageDataChanged;

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
        /// The execution manager.
        /// </summary>
        private readonly ExecutionManager executionManager;

        /// <summary>
        /// The cancellation Token source used to cancel operations.
        /// </summary>
        public CancellationTokenSource CancellationTokenSource { get; }

        /// <summary>
        /// Event fired when AvailableNodes is updated.
        /// </summary>
        public event Func<NodeBase, Task> AvailableNodesUpdated;

        /// <summary>
        /// Gets or sets the available nodes.
        /// </summary>
        public IDictionary<string, NodeBase> AvailableNodes { get; set; } = new Dictionary<string, NodeBase>();

        /// <summary>
        /// Initializes a new instance of the <see cref="NodePageManager"/> class.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="executionManager">The execution manager.</param>
        /// <param name="nodeJSEnvironmentManager">The NodeJS environment manager.</param>
        public NodePageManager(ILoggerFactory loggerFactory, ExecutionManager executionManager, NodeJSEnvironmentManager nodeJSEnvironmentManager)
        {
            this.loggerFactory = loggerFactory;
            this.executionManager = executionManager;
            
            NodeJSEnvironmentManager = nodeJSEnvironmentManager;
        }

        /// <summary>
        /// Creates a new node from the specified plugin.
        /// </summary>
        /// <param name="pluginBase">The plugin base to create the node from.</param>
        /// <param name="attribute">The plugin attribute.</param>
        /// <param name="isStartup">Indicates whether the node is a startup node.</param>
        /// <returns>A task representing the asynchronous operation, with the created node as the result.</returns>
        public async Task<NodeBase> CreateNode(Type pluginBase, NovusPluginAttribute attribute, NodeUIConfig uiConfig = null, string id = null, bool isStartup = false)
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

                if (uiConfig == null) uiConfig = new NodeUIConfig();

                NodeBase node = new(logger, id, plugin, uiConfig, executionManager, attribute, this, NodeJSEnvironmentManager, DebugLogChanged);

                AvailableNodes.Add(node.Id, node);
                
                if(!isStartup)
                    await OnAvailableNodesUpdated(node).ConfigureAwait(false);

                if (!isStartup && OnPageDataChanged != null)
                {
                    await OnPageDataChanged.Invoke().ConfigureAwait(false);
                }

                node.PropertyChanged += Node_PropertyChanged;

                return node;
            }
            return null;
        }

        /// <summary>
        /// Handles the PropertyChanged event of a node.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Node_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (OnPageDataChanged != null)
            {
                //Runs the save of a node when NodeUI Data changes / this happens a lot, so we do not wait for it. Just create a task an fire away. 
                Task.Run(async () => await OnPageDataChanged.Invoke().ConfigureAwait(false));
            }
        }

        /// <summary>
        /// Creates a new connection between two nodes.
        /// </summary>
        /// <param name="sourceId">The ID of the source node.</param>
        /// <param name="sourcePortId">The ID of the source port.</param>
        /// <param name="targetId">The ID of the target node.</param>
        /// <param name="targetPortId">The ID of the target port.</param>
        public async Task NewConnection(string sourceId, string sourcePortId, string targetId, string targetPortId, bool isStartup = false)
        {
            if (AvailableNodes.TryGetValue(sourceId, out var sourceNode) && AvailableNodes.TryGetValue(targetId, out var targetNode))
            {
                sourceNode.OutputPorts[sourcePortId].AddConnection(targetPortId, targetNode);

                if (!isStartup && OnPageDataChanged != null)
                {
                    await OnPageDataChanged.Invoke().ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Removes the connection between the specified source and target nodes.
        /// </summary>
        /// <param name="sourceId">The ID of the source node.</param>
        /// <param name="sourcePortId">The ID of the source port.</param>
        /// <param name="targetId">The ID of the target node.</param>
        /// <param name="targetPortId">The ID of the target port.</param>
        public async Task RemoveConnection(string sourceId, string sourcePortId, string targetId, string targetPortId)
        {
            if (AvailableNodes.TryGetValue(sourceId, out var sourceNode))
            {
                sourceNode.OutputPorts[sourcePortId].RemoveConnection(targetPortId);
            }
            
            if (OnPageDataChanged != null)
            {
                await OnPageDataChanged.Invoke().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Removes the specified node and all its connections.
        /// </summary>
        /// <param name="id">The ID of the node to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ElementRemoved(string id)
        {
            if (AvailableNodes.TryGetValue(id, out var node))
            {
                try
                {
                    //Stop node operations
                    await node.CloseNodeAsync().ConfigureAwait(false);
                }
                catch { }

                //Remove event handler
                node.PropertyChanged -= Node_PropertyChanged;

                AvailableNodes.Remove(id);

                // Remove connections from output ports
                foreach (var outputPort in node.OutputPorts)
                {
                    outputPort.Value.RemoveAllConnections();
                }

                // Remove connections from input port
                node.InputPort?.RemoveAllConnections();

                if (OnPageDataChanged != null)
                {
                    await OnPageDataChanged.Invoke().ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Invokes the AvailableNodesUpdated event.
        /// </summary>
        /// <param name="nodeBase">The node that was updated.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task OnAvailableNodesUpdated(NodeBase nodeBase)
        {
            if(AvailableNodesUpdated != null)
                await AvailableNodesUpdated.Invoke(nodeBase).ConfigureAwait(false);
        }
    }
}
