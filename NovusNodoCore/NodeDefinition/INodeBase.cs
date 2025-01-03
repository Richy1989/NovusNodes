using System.Collections.Generic;
using System.Threading.Tasks;
using NovusNodoPluginLibrary;

namespace NovusNodoCore.NodeDefinition
{
    /// <summary>
    /// Represents the base interface for all nodes.
    /// </summary>
    public interface INodeBase : IPluginBase
    {
        /// <summary>
        /// Gets the plugin base instance.
        /// </summary>
        public IPluginBase PluginBase { get; }

        /// <summary>
        /// Gets or sets the input port of the node.
        /// </summary>
        InputPort InputPort { get; set; }

        /// <summary>
        /// Gets or sets the output ports of the node.
        /// </summary>
        Dictionary<string, OutputPort> OutputPorts { get; set; }

        /// <summary>
        /// Gets or sets the UI configuration of the node.
        /// </summary>
        NodeUIConfig UIConfig { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the node is enabled.
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Executes the node with the provided JSON data.
        /// </summary>
        /// <param name="jsonData">The JSON data to be processed by the node.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ExecuteNode(string jsonData);
    }
}