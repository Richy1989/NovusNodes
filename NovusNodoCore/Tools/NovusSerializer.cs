using System.Text.Json;
using NovusNodoCore.NodeDefinition;
using NovusNodoCore.SaveData;

namespace NovusNodoCore.Tools
{
    public class NovusSerializer
    {
        public static async Task<NodeSaveModel> CreateNodeSaveModel(NodeBase node)
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

            // Add the connected ports
            foreach (var outputPort in node.InputPort.ConnectedOutputPort.Values)
            {
                nodeSave.ConnectedPorts.Add(new ConnectionModel { NodeId = outputPort.Node.Id, PortId = outputPort.Id });
            }

            return nodeSave;
        }


        /// <summary>
        /// Translated all Ids to new ones, which will create a new set of connected nodes, which can be added to the project. 
        /// Used for Copy Paste functionality.
        /// </summary>
        /// <param name="nodeSaveModels"></param>
        public void CreateIdTranslatedModel(List<NodeSaveModel> nodeSaveModels)
        {
            // Create a dictionary to store the translated models
            var idTranslation = new Dictionary<string, string>();

            foreach (var nodeSaveModel in nodeSaveModels)
            {
                nodeSaveModel.NodeId = GetTranslatedId(nodeSaveModel.NodeId, idTranslation);

                nodeSaveModel.InputPortId = GetTranslatedId(nodeSaveModel.InputPortId, idTranslation);

                List<string> newOutputPorts = new();
                foreach (var outputPortId in nodeSaveModel.OutputPortsIds)
                {
                    newOutputPorts.Add(GetTranslatedId(outputPortId, idTranslation));
                }
                nodeSaveModel.OutputPortsIds = newOutputPorts;

                foreach(var connectedPort in nodeSaveModel.ConnectedPorts)
                {
                    connectedPort.NodeId = GetTranslatedId(connectedPort.NodeId, idTranslation);
                    connectedPort.PortId = GetTranslatedId(connectedPort.PortId, idTranslation);
                }
            }
        }

        private string GetTranslatedId(string id, Dictionary<string, string> idTranslation)
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
