using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;

namespace NovusNodoPluginLibrary
{
    /// <summary>
    /// Represents the base interface for all plugins.
    /// </summary>
    public interface IPluginBase
    {
        /// <summary>
        /// Gets the unique identifier for the plugin.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets or sets the type of the UIType component associated with the plugin.
        /// </summary>
        Type UIType { get; set; }

        /// <summary>
        /// Gets or sets the configuration object.
        /// </summary>
        object PluginConfig { get; set; }

        /// <summary>
        /// Gets or sets the parent node.
        /// </summary>
        IPluginBase ParentNode { get; set; }

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
        /// Gets the dictionary of work tasks to be executed by the node.
        /// </summary>
        IDictionary<string, Func<JsonObject, Task<JsonObject>>> WorkTasks { get; }

        /// <summary>
        /// Gets or sets the callback function to execute JavaScript code.
        /// </summary>
        /// <value>
        /// A function that takes a string and a <see cref="JsonObject"/> as parameters and returns a <see cref="Task{JsonObject}"/>.
        /// </value>
        Func<string, JsonObject, Task<JsonObject>> ExecuteJavaScriptCodeCallback { get; set; }
    }
}
