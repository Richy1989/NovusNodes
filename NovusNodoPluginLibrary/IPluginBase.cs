using System.Drawing;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;

namespace NovusNodoPluginLibrary
{
    /// <summary>
    /// Represents the base interface for all plugins.
    /// </summary>
    public interface IPluginBase
    {
        Func<Task> SaveSettings { get; set; }
        /// <summary>
        /// Gets or sets the logger instance for the plugin.
        /// </summary>
        ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets the type of the UI component associated with the plugin.
        /// </summary>
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
        /// Gets or sets the name of the plugin.
        /// </summary>
        string Name { get; set; }

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
        /// Gets the dictionary of work tasks to be executed by the node.
        /// </summary>
        IDictionary<string, Func<JsonObject, Task<JsonObject>>> WorkTasks { get; }

        Func<string, JsonObject, Task<JsonObject>> ExecuteJavaScriptCodeCallback { get; set; }
    }
}
