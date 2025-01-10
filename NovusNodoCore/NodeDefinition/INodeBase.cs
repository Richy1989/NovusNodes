using System.Text.Json.Nodes;
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
        public PluginBase PluginBase { get; }

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
        /// Executes the node's workload if the node is enabled, and then triggers the execution of the next nodes.
        /// </summary>
        /// <param name="jsonData">The JSON data to be processed by the node.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task ExecuteNode(JsonObject jsonData);
    }
}