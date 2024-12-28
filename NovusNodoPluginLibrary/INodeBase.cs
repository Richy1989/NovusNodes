namespace NovusNodoPluginLibrary
{
    /// <summary>
    /// Represents the base interface for all nodes.
    /// </summary>
    public interface INodeBase : IPluginBase
    {

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