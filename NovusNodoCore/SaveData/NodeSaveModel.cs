using NovusNodoCore.NodeDefinition;

namespace NovusNodoCore.SaveData
{
    public class ConnectionModel
    {
        public string NodeId { get; set; }
        public string PortId { get; set; }
    }
    /// <summary>
    /// Represents the model used to save the state of a node.
    /// </summary>
    public class NodeSaveModel
    {
        public List<string> OutputNodes { get; set; } = [];
        public string InputPortId { get; set; }
        /// <summary>
        /// Gets or sets the list of connected output ports to this input port.
        /// Each tuple contains the NodeId, and PortId.
        /// </summary>
        public List<ConnectionModel> ConnectedPorts { get; set; }

        /// <summary>
        /// Gets or sets the PageId of the node.
        /// </summary>
        public string PageId { get; set; }

        /// <summary>
        /// Gets or sets the NodeId of the node.
        /// </summary>
        public string NodeId { get; set; }

        /// <summary>
        /// Gets or sets the configuration for the UI of the node.
        /// </summary>
        public NodeUIConfig NodeConfig { get; set; }

        /// <summary>
        /// Gets or sets the configuration for the plugin associated with the node.
        /// </summary>
        public object PluginConfig { get; set; }

        /// <summary>
        /// Gets or sets the base ID of the plugin.
        /// </summary>
        public string PluginBaseId { get; set; }
    }
}
