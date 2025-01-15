using System.Drawing;
using NovusNodoCore.NodeDefinition;
using NovusNodoPluginLibrary;

namespace NovusNodoCore.SaveData
{
    /// <summary>
    /// Represents the model used to save the state of a node.
    /// </summary>
    internal class NodeSaveModel
    {
        /// <summary>
        /// Gets or sets the input port of the node.
        /// </summary>
        public InputPort InputPort { get; set; }

        /// <summary>
        /// Gets or sets the output ports of the node.
        /// </summary>
        public Dictionary<string, OutputPort> OutputPorts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the node is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the node.
        /// </summary>
        public string NodeID { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the page containing the node.
        /// </summary>
        public string PageID { get; set; }

        /// <summary>
        /// Gets or sets the name of the page containing the node.
        /// </summary>
        public string PageName { get; set; }

        /// <summary>
        /// Gets or sets the background color of the node.
        /// </summary>
        public Color BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the UI configuration of the node.
        /// </summary>
        public NodeUIConfig NodeUIConfig { get; set; }

        /// <summary>
        /// Gets or sets the type of the node.
        /// </summary>
        public NodeType NodeType { get; set; }

        /// <summary>
        /// Gets or sets the base type of the plugin associated with the node.
        /// </summary>
        public Type PluginBaseType { get; set; }
    }
}
