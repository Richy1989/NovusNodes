using NovusNodoPluginLibrary;

namespace NovusNodoCore.NodeDefinition
{
    /// <summary>
    /// Represents the base interface for all nodes.
    /// </summary>
    public interface INodeBase : IPluginBase
    {
        NodeUIConfig UIConfig { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the node is enabled.
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Gets the dictionary of next nodes.
        /// </summary>
        IDictionary<string, INodeBase> NextNodes { get; }

        /// <summary>
        /// Executes the node.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ExecuteNode(string jsonData);
    }
}