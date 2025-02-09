using System.Text.Json;
using Microsoft.Extensions.Logging;
using NovusNodoCore.Managers;
using NovusNodoCore.NodeDefinition;
using NovusNodoCore.SaveData;

namespace NovusNodoCore.Tools
{
    /// <summary>
    /// Provides methods to create and load node models.
    /// </summary>
    public class NovusModelCreator
    {
        private readonly ExecutionManager _executionManager;
        private readonly ILogger<NovusModelCreator> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NovusModelCreator"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="executionManager">The execution manager instance.</param>
        public NovusModelCreator(ILogger<NovusModelCreator> logger, ExecutionManager executionManager)
        {
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
            var nodeSave = new NodeSaveModel
            {
                NodeId = node.Id,
                InputPortId = node.InputPort?.Id,
                PluginBaseId = node.PluginBase.Id,
                NodeConfig = node.UIConfig.Clone(),
                ConnectedPorts = [],
                PluginSettings = node.PluginSettings.Clone(),
            };

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
                nodeSave.PluginConfig = node.PluginConfig != null ? (string)node.PluginConfig : null;
            }

            foreach (var outputport in node.OutputPorts)
            {
                PortSaveModel portSave = new()
                {
                    PortId = outputport.Value.Id,
                    RelatedWorkerTaskId = outputport.Value.RelatedWorkerTaskId
                };

                nodeSave.OutputPorts.Add(portSave);
            }

            if (node.InputPort != null)
            {
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
            await LoadNodes(pageModel.Nodes, nodePage).ConfigureAwait(false);
            await LoadLinks(pageModel.Nodes, pageModel.PageId, false, true).ConfigureAwait(false);

            _logger.LogInformation("Project loaded successfully.");
        }

        /// <summary>
        /// Loads the nodes from a list of NodeSaveModel instances asynchronously.
        /// </summary>
        /// <param name="nodeSaveModelList">The list of NodeSaveModel instances to load.</param>
        /// <param name="nodePageManager">The NodePageManager instance to manage the nodes.</param>
        /// <returns>A task representing the asynchronous load operation.</returns>
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

                node.PluginSettings = nodeModel.PluginSettings;

                if (pluginBaseAttribute.PluginConfigType != null)
                {
                    var config = JsonSerializer.Deserialize(nodeModel.PluginConfig, pluginBaseAttribute.PluginConfigType);
                    config = Convert.ChangeType(config, pluginBaseAttribute.PluginConfigType);
                    node.PluginBase.PluginConfig = config;
                }
                else
                {
                    node.PluginBase.PluginConfig = (string)nodeModel.PluginConfig;
                }

                if (nodeModel.InputPortId != null)
                {
                    node.CreateInputPort(nodeModel.InputPortId);
                }

                node.OutputPorts = [];
                foreach (var outputPortModel in nodeModel.OutputPorts)
                {
                    node.AddOutputPort(outputPortModel.PortId, outputPortModel.RelatedWorkerTaskId);
                }

                await nodePageManager.OnAvailableNodesUpdated(node).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Loads the links between nodes asynchronously.
        /// </summary>
        /// <param name="nodeSaveModelList">The list of NodeSaveModel instances containing the nodes and their connections.</param>
        /// <param name="pageId">The ID of the page to load the links for.</param>
        /// <param name="createSubset">Indicates whether to create a subset of the nodes.</param>
        /// <param name="isStartup">Indicates whether the operation is during startup.</param>
        /// <returns>A task representing the asynchronous load operation.</returns>
        public async Task LoadLinks(List<NodeSaveModel> nodeSaveModelList, string pageId, bool createSubset, bool isStartup)
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

                    if (createSubset)
                    {
                        if (nodeSaveModelList.Find(x => x.NodeId == connection.NodeId) == null)
                        {
                            continue;
                        }
                    }

                    _logger.LogDebug($"Creating connection from Node: {node.Id} Port {node.InputPort.Id} to Node: {connection.NodeId} Port {connection.PortId}");
                    await nodePageManager.NewConnection(connection.NodeId, connection.PortId, node.Id, node.InputPort.Id, isStartup).ConfigureAwait(false);
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
            var idTranslation = new Dictionary<string, string>();

            foreach (var nodeSaveModel in nodeSaveModels)
            {
                nodeSaveModel.NodeId = GetTranslatedId(nodeSaveModel.NodeId, idTranslation);

                if (nodeSaveModel.InputPortId != null)
                    nodeSaveModel.InputPortId = GetTranslatedId(nodeSaveModel.InputPortId, idTranslation);

                List<PortSaveModel> newOutputPorts = [];
                foreach (var outputPortModel in nodeSaveModel.OutputPorts)
                {
                    PortSaveModel translatedPortSaveModel = new()
                    {
                        PortId = GetTranslatedId(outputPortModel.PortId, idTranslation),
                        RelatedWorkerTaskId = outputPortModel.RelatedWorkerTaskId
                    };

                    //Do not translate the WorkerTaskId / These are constant.
                    newOutputPorts.Add(translatedPortSaveModel);
                }
                nodeSaveModel.OutputPorts = newOutputPorts;

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
            if (idTranslation.TryGetValue(id, out string value))
            {
                return value;
            }
            var newId = Guid.NewGuid().ToString();
            idTranslation.Add(id, newId);
            return newId;
        }
    }
}
