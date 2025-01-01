using System.Drawing;
using System.Text.Json;

namespace NovusNodoPluginLibrary
{
    /// <summary>
    /// Represents the base class for all plugins.
    /// </summary>
    /// <typeparam name="ConfigType">The type of the configuration object.</typeparam>
    /// <remarks>
    /// Initializes a new instance of the <see cref="PluginBase{ConfigType}"/> class.
    /// </remarks>
    /// <param name="nodeType">The type of the node.</param>
    public abstract class PluginBase : IPluginBase
    {
        public PluginBase()
        {
            WorkTasks = new Dictionary<string, Func<string, Task<string>>>();
        }
        /// <summary>
        /// Gets the unique identifier for the plugin.
        /// </summary>
        public virtual string ID { get; }

        /// <summary>
        /// Gets the name of the plugin.
        /// </summary>
        public virtual string Name { get; }

        /// <summary>
        /// Gets the background color of the plugin.
        /// </summary>
        public virtual Color Background { get; }

        /// <summary>
        /// Gets the type of the node.
        /// </summary>
        public virtual NodeType NodeType { get; }

        /// <summary>
        /// Gets or sets the JSON configuration.
        /// </summary>
        public required string JsonConfig { get; set; }

        /// <summary>
        /// Gets or sets the parent node.
        /// </summary>
        public required IPluginBase ParentNode { get; set; }

        public IDictionary<string, Func<string, Task<string>>> WorkTasks { get; }

        public void AddWorkTask(Func<string, Task<string>> task)
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
        /// Defines the workload to be executed by the node.
        /// </summary>
        /// <param name="jsonData">The JSON data to be processed by the workload.</param>
        /// <returns>A function that represents the asynchronous operation and returns a string.</returns>
        //public virtual Func<Task<string>> Workload(string jsonData)
        //{
        //    throw new NotImplementedException();
        //}

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
