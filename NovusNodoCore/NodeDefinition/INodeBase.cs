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
        /// Gets or sets the input port of the node.
        /// </summary>
        public InputPort InputPort { get; set; }
        /// <summary>
        /// Gets or sets the output port of the node.
        /// </summary>
        public Dictionary<string, OutputPort> OutputPorts { get; set; }

        NodeUIConfig UIConfig { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the node is enabled.
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Executes the node.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ExecuteNode(string jsonData);
    }
}