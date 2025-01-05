using System.Drawing;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;

namespace NovusNodoPluginLibrary
{
    /// <summary>
    /// Represents the base class for all plugins.
    /// </summary>
    public abstract class PluginBase : IPluginBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginBase"/> class.
        /// </summary>
        public PluginBase()
        {
            WorkTasks = new Dictionary<string, Func<JsonObject, Task<JsonObject>>>();
        }

        public Func<Task> SaveSettings { get; set; }

        public Func<string, JsonObject, Task<JsonObject>> ExecuteJavaScriptCodeCallback { get; set; }
        public Func<string, JsonObject, Task> UpdateDebugLog { get; set; }

        /// <summary>
        /// Gets or sets the logger instance for the plugin.
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets the UI type associated with the plugin.
        /// </summary>
        public Type UI { get; set; }

        /// <summary>
        /// Gets the unique identifier for the plugin.
        /// </summary>
        public abstract string ID { get; }

        /// <summary>
        /// Gets the name of the plugin.
        /// </summary>
        public abstract string Name { get; set; }

        /// <summary>
        /// Gets the background color of the plugin.
        /// </summary>
        public abstract Color Background { get; }

        /// <summary>
        /// Gets the type of the node.
        /// </summary>
        public abstract NodeType NodeType { get; }

        /// <summary>
        /// Gets or sets the JSON configuration.
        /// </summary>
        public string JsonConfig { get; set; } = "";

        /// <summary>
        /// Gets or sets the parent node.
        /// </summary>
        public IPluginBase ParentNode { get; set; }

        /// <summary>
        /// Gets the dictionary of work tasks.
        /// </summary>
        public IDictionary<string, Func<JsonObject, Task<JsonObject>>> WorkTasks { get; } = new Dictionary<string, Func<JsonObject, Task<JsonObject>>>();

        /// <summary>
        /// Adds a work task to the dictionary.
        /// </summary>
        /// <param name="task">The work task to add.</param>
        public void AddWorkTask(Func<JsonObject, Task<JsonObject>> task)
        {
            WorkTasks.Add(Guid.NewGuid().ToString(), task);
        }

        /// <summary>
        /// Prepares the workload asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual Task PrepareWorkloadAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Extracts a variable from the JSON configuration.
        /// </summary>
        /// <param name="jsonConfig">The JSON configuration string.</param>
        /// <param name="variable">The variable to extract.</param>
        /// <returns>The value of the extracted variable as a string.</returns>
        public static string ExtractVariable(string jsonConfig, string variable)
        {
            var jsonDocument = JsonDocument.Parse(jsonConfig);
            if (jsonDocument.RootElement.TryGetProperty(variable, out var debugPathElement))
            {
                return debugPathElement.GetString() ?? string.Empty;
            }
            return string.Empty;
        }
    }
}
