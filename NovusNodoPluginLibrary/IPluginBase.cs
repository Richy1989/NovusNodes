using System.Drawing;

namespace NovusNodoPluginLibrary
{
    /// <summary>
    /// Represents the base interface for all plugins.
    /// </summary>
    public interface IPluginBase
    {
        Type UI { get; set; }
        /// <summary>
        /// Gets or sets the JSON configuration.
        /// </summary>
        string JsonConfig { get; set; }

        /// <summary>
        /// Gets or sets the parent node.
        /// </summary>
        IPluginBase ParentNode { get; set; }

        /// <summary>
        /// Gets the unique identifier for the plugin.
        /// </summary>
        string ID { get; }

        /// <summary>
        /// Gets the name of the plugin.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the background color of the plugin.
        /// </summary>
        Color Background { get; }

        /// <summary>
        /// Gets the type of the node.
        /// </summary>
        NodeType NodeType { get; }

        /// <summary>
        /// Prepares the workload asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task PrepareWorkloadAsync();

        /// <summary>
        /// Defines the workload to be executed by the node.
        /// </summary>
        /// <param name="jsonData">The JSON data to be processed by the workload.</param>
        /// <returns>A function that represents the asynchronous operation and returns a string result.</returns>
        //Func<Task<string>> Workload(string jsonData);

        IDictionary<string, Func<string, Task<string>>> WorkTasks { get; }
    }
}
