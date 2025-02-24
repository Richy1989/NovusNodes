﻿using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using NovusNodoCore.DebugNotification;
using NovusNodoCore.Extensions;
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
        /// Event fired when DebugLog is updated.
        /// </summary>
        public Func<string, JsonObject, Task> DebugLogChanged;

        /// <summary>
        /// Event fired when the project is changed, to trigger a save file.
        /// </summary>
        public event Func<Task> OnPageDataChanged;

        /// <summary>
        /// Event fired when AvailableNodes is updated.
        /// </summary>
        public event Func<NodeBase, Task> AvailableNodesUpdated;

        public event Func<string, Task> OnPortsChanged;

        /// <summary>
        /// The NodeJS environment manager.
        /// </summary>
        private readonly NodeJSEnvironmentManager NodeJSEnvironmentManager;

        /// <summary>
        /// The logger factory.
        /// </summary>
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// The execution manager.
        /// </summary>
        private readonly ExecutionManager executionManager;

        private readonly ILogger<NodePageManager> _logger;

        /// <summary>
        /// The cancellation Token source used to cancel operations.
        /// </summary>
        public CancellationTokenSource CancellationTokenSource { get; }

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
            _logger = loggerFactory.CreateLogger<NodePageManager>();
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
                if (uiConfig == null) uiConfig = new NodeUIConfig();

                NodeBase node = new(loggerFactory, id, plugin, uiConfig, executionManager, attribute, this, NodeJSEnvironmentManager, DebugLogChanged);

                AvailableNodes.Add(node.Id, node);
                
                if(!isStartup)
                    await OnAvailableNodesUpdated(node).ConfigureAwait(false);

                if (!isStartup)
                {
                    await OnPageDataChanged.RaiseAsync().ConfigureAwait(false);
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
            _logger.LogDebug($"Node Property Changed: {e.PropertyName}");
            //Runs the save of a node when NodeUI Data changes / this happens a lot, so we do not wait for it. Just create a task an fire away. 
            _ = OnPageDataChanged.RaiseAsync().ConfigureAwait(false);
            //Task.Run(async () => await OnPageDataChanged.RaiseAsync().ConfigureAwait(false));
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
            _logger.LogDebug($"New Connection: {sourceId} - {sourcePortId} -> {targetId} - {targetPortId}");
            if (AvailableNodes.TryGetValue(sourceId, out var sourceNode) && AvailableNodes.TryGetValue(targetId, out var targetNode))
            {
                sourceNode.OutputPorts[sourcePortId].AddConnection(targetPortId, targetNode);

                if (!isStartup)
                {
                    await OnPageDataChanged.RaiseAsync().ConfigureAwait(false);
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
            _logger.LogDebug($"Remove Connection: {sourceId} - {sourcePortId} -> {targetId} - {targetPortId}");
            if (AvailableNodes.TryGetValue(sourceId, out var sourceNode))
            {
                sourceNode.OutputPorts[sourcePortId].RemoveConnection(targetPortId);
            }

            await OnPageDataChanged.RaiseAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Removes the specified node and all its connections.
        /// </summary>
        /// <param name="id">The ID of the node to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ElementRemoved(string id)
        {
            _logger.LogDebug($"Element Removed: {id}");
            if (AvailableNodes.TryGetValue(id, out var node))
            {
                try
                {
                    //Stop node operations
                    await node.CloseNodeAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    string a = ex.Message;
                }

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

                await OnPageDataChanged.RaiseAsync().ConfigureAwait(false);
            }
        }

        public async Task PortsChanged(string id)
        {
            await RaisePortsChanged(id).ConfigureAwait(false);
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

        public async Task RaisePortsChanged(string nodeId)
        {
            var handlers = OnPortsChanged?.GetInvocationList(); // Get all subscribers
            if (handlers == null) return;

            foreach (var handler in handlers.Cast<Func<string, Task>>())
            {
                await handler(nodeId).ConfigureAwait(false);
            }
        }
    }
}
