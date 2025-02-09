using NovusNodoCore.NodeDefinition;
using NovusNodoPluginLibrary;

namespace NovusNodoCore.SaveData
{
    /// <summary>
    /// Represents a connection model containing NodeId and PortId.
    /// </summary>
    public class ConnectionModel
    {
        /// <summary>
        /// Gets or sets the NodeId of the connection.
        /// </summary>
        public string NodeId { get; set; }

        /// <summary>
        /// Gets or sets the PortId of the connection.
        /// </summary>
        public string PortId { get; set; }
    }
    /// <summary>
    /// Represents the model used to save the state of a node.
    /// </summary>
    public class NodeSaveModel
    {
        /// <summary>
        /// Gets or sets the PageId of the node.
        /// </summary>
        public string PageId { get; set; }

        /// <summary>
        /// Gets or sets the NodeId of the node.
        /// </summary>
        public string NodeId { get; set; }

        /// <summary>
        /// Gets or sets the list of output ports ids.
        /// </summary>
        public List<PortSaveModel> OutputPorts { get; set; } = [];

        /// <summary>
        /// Gets or sets the InputPortId of the node.
        /// </summary>
        public string InputPortId { get; set; }

        /// <summary>
        /// Gets or sets the list of connected output ports to this input port.
        /// Each tuple contains the NodeId, and PortId.
        /// </summary>
        public List<ConnectionModel> ConnectedPorts { get; set; }

        /// <summary>
        /// Gets or sets the configuration for the UI of the node.
        /// </summary>
        public NodeUIConfig NodeConfig { get; set; }

        /// <summary>
        /// Gets or sets the configuration for the plugin associated with the node.
        /// </summary>
        public string PluginConfig { get; set; }

        /// <summary>
        /// Gets or sets the base ID of the plugin.
        /// </summary>
        public string PluginBaseId { get; set; }

        /// <summary>
        /// Gets or sets the settings for the plugin.
        /// </summary>
        public PluginSettings PluginSettings { get; set; }
    }
}
