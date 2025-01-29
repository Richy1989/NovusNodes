using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using NovusNodoCore.Managers;
using NovusNodoCore.NodeDefinition;
using NovusNodoCore.SaveData;

namespace NovusNodoCore.Tools
{
    public class NovusModelCreator
    {
        private readonly ExecutionManager _executionManager;
        private ILogger<NovusModelCreator> _logger;

        public NovusModelCreator(ILogger<NovusModelCreator> logger, ExecutionManager executionManager) {
            this._executionManager = executionManager;
            this._logger = logger;
        }

        /// <summary>
        /// Creates a NodeSaveModel from a given NodeBase instance.
        /// </summary>
        /// <param name="node">The node to create the save model from.</param>
        /// <returns>A task representing the asynchronous operation, with the created NodeSaveModel as the result.</returns>
        public async Task<NodeSaveModel> CreateNodeSaveModel(NodeBase node)
        {
            // Create a new nodeModel save model
            var nodeSave = new NodeSaveModel
            {
                NodeId = node.Id,
                InputPortId = node.InputPort?.Id,
                PluginBaseId = node.PluginBase.Id,
                NodeConfig = node.UIConfig.Clone(),
                ConnectedPorts = [],
                PluginSettings = node.PluginSettings.Clone(),
            };

            // If we have a config type, serialize and add the config
            if (node.PluginIdAttribute.PluginConfigType != null)
            {
                using var memoryStream = new MemoryStream();
                await JsonSerializer.SerializeAsync(memoryStream, node.PluginConfig, node.PluginIdAttribute.PluginConfigType).ConfigureAwait(false);
                memoryStream.Position = 0;
                using var streamReader = new StreamReader(memoryStream);
                nodeSave.PluginConfig = await streamReader.ReadToEndAsync().ConfigureAwait(false);
            }
            else
            {
                //Otherwise, just set the config as a string
                nodeSave.PluginConfig = node.PluginConfig != null ? (string)node.PluginConfig : null;
            }

            // Add the output ports
            foreach (var outputport in node.OutputPorts)
            {
                nodeSave.OutputPortsIds.Add(outputport.Value.Id);
            }

            if (node.InputPort != null)
            {
                // Add the connected ports
                foreach (var outputPort in node.InputPort.ConnectedOutputPort.Values)
                {
                    nodeSave.ConnectedPorts.Add(new ConnectionModel { NodeId = outputPort.Node.Id, PortId = outputPort.Id });
                }
            }

            return nodeSave;
        }

        /// <summary>
        /// Loads a specific page asynchronously.
        /// </summary>
        /// <param name="pageModel">The model of the page to load.</param>
        /// <returns>A task representing the asynchronous load operation.</returns>
        public async Task LoadPage(PageSaveModel pageModel)
        {
            var nodePage = await _executionManager.AddNewTab(pageModel.PageId, true).ConfigureAwait(false);

            nodePage.PageName = pageModel.PageName;
            // Load the nodes
            await LoadNodes(pageModel.Nodes, nodePage).ConfigureAwait(false);

            // Load the links
            await LoadLinks(pageModel.Nodes, pageModel.PageId, false, true, false).ConfigureAwait(false);
            
            _logger.LogInformation("Project loaded successfully.");
        }

        /// <summary>
        /// Loads the nodes from a list of NodeSaveModel instances asynchronously.
        /// </summary>
        /// <param name="nodeSaveModelList"></param>
        /// <param name="nodePageManager"></param>
        /// <returns></returns>
        public async Task LoadNodes(List<NodeSaveModel> nodeSaveModelList, NodePageManager nodePageManager)
        {
            foreach (var nodeModel in nodeSaveModelList)
            {
                if (!_executionManager.AvailablePlugins.ContainsKey(nodeModel.PluginBaseId))
                {
                    _logger.LogError($"Plugin with ID {nodeModel.PluginBaseId} not found.");
                    continue;
                }

                var pluginBaseType = _executionManager.AvailablePlugins[nodeModel.PluginBaseId].Item1;
                var pluginBaseAttribute = _executionManager.AvailablePlugins[nodeModel.PluginBaseId].Item2;

                NodeUIConfig uiConfig = new();
                uiConfig.CopyFrom(nodeModel.NodeConfig);
                
                var node = await nodePageManager.CreateNode(pluginBaseType, pluginBaseAttribute, uiConfig, nodeModel.NodeId, true).ConfigureAwait(false);
                
                // Set the plugin settings
                node.PluginSettings = nodeModel.PluginSettings;

                // I we have a config type, deserialize the config
                if (pluginBaseAttribute.PluginConfigType != null)
                {
                    var config = JsonSerializer.Deserialize(nodeModel.PluginConfig, pluginBaseAttribute.PluginConfigType);
                    config = Convert.ChangeType(config, pluginBaseAttribute.PluginConfigType);
                    node.PluginBase.PluginConfig = config;

                }
                else
                {
                    //Otherwise, just set the config as a string
                    node.PluginBase.PluginConfig = (string)nodeModel.PluginConfig;
                }

                if (nodeModel.InputPortId != null)
                {   // Create the input port, replace auto created ones
                    node.CreateInputPort(nodeModel.InputPortId);
                }

                // Create the output ports, replace auto created ones
                node.OutputPorts = new Dictionary<string, OutputPort>();
                foreach (var outputNodeId in nodeModel.OutputPortsIds)
                {
                    node.AddOutputPort(outputNodeId);
                }

                // When the node is created, or copied, we need to define it fully first, then announce it to the page
                await nodePageManager.OnAvailableNodesUpdated(node).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Loads the links between nodes asynchronously.
        /// </summary>
        /// <param name="pages">The list of pages containing the nodes and their connections.</param>
        /// <returns>A task representing the asynchronous load operation.</returns>
        public async Task LoadLinks(List<NodeSaveModel> nodeSaveModelList, string pageId, bool createSubset, bool isStartup, bool forceRedraw)
        {
            if (!_executionManager.NodePages.TryGetValue(pageId, out NodePageManager nodePageManager))
            {
                _logger.LogError($"Tried to load non existing page, ID: {pageId}");
                return;
            }

            foreach (var nodeModel in nodeSaveModelList)
            {
                if (nodeModel.ConnectedPorts == null) continue;

                foreach (var connection in nodeModel.ConnectedPorts)
                {

                    var node = nodePageManager.AvailableNodes[nodeModel.NodeId];

                    //Check if we should only load a subset of the nodes
                    if (createSubset)
                    {
                        //Check if nodeId is in the nodeSaveModelList
                        if (nodeSaveModelList.Find(x => x.NodeId == connection.NodeId) == null)
                        {
                            continue;
                        }
                    }

                    _logger.LogDebug($"Creating connection from Node: {node.Id} Port {node.InputPort.Id} to Node: {connection.NodeId} Port {connection.PortId}");
                    await nodePageManager.NewConnection(connection.NodeId, connection.PortId, node.Id, node.InputPort.Id, isStartup, forceRedraw).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Translates all IDs to new ones, creating a new set of connected nodes that can be added to the project.
        /// Used for Copy Paste functionality.
        /// </summary>
        /// <param name="nodeSaveModels">The list of NodeSaveModel instances to translate.</param>
        public void CreateIdTranslatedModel(List<NodeSaveModel> nodeSaveModels)
        {
            // Create a dictionary to store the translated models
            var idTranslation = new Dictionary<string, string>();

            foreach (var nodeSaveModel in nodeSaveModels)
            {
                nodeSaveModel.NodeId = GetTranslatedId(nodeSaveModel.NodeId, idTranslation);

                if(nodeSaveModel.InputPortId != null)
                    nodeSaveModel.InputPortId = GetTranslatedId(nodeSaveModel.InputPortId, idTranslation);

                List<string> newOutputPorts = new();
                foreach (var outputPortId in nodeSaveModel.OutputPortsIds)
                {
                    newOutputPorts.Add(GetTranslatedId(outputPortId, idTranslation));
                }
                nodeSaveModel.OutputPortsIds = newOutputPorts;

                foreach (var connectedPort in nodeSaveModel.ConnectedPorts)
                {
                    connectedPort.NodeId = GetTranslatedId(connectedPort.NodeId, idTranslation);
                    connectedPort.PortId = GetTranslatedId(connectedPort.PortId, idTranslation);
                }

            }
        }

        /// <summary>
        /// Gets a translated ID for a given ID, creating a new one if it doesn't exist in the translation dictionary.
        /// </summary>
        /// <param name="id">The original ID to translate.</param>
        /// <param name="idTranslation">The dictionary storing the ID translations.</param>
        /// <returns>The translated ID.</returns>
        private static string GetTranslatedId(string id, Dictionary<string, string> idTranslation)
        {
            if (idTranslation.ContainsKey(id))
            {
                return idTranslation[id];
            }
            var newId = Guid.NewGuid().ToString();
            idTranslation.Add(id, newId);
            return newId;
        }
    }
}
