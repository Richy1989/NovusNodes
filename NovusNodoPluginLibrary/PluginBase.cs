using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;

namespace NovusNodoPluginLibrary
{
    /// <summary>
    /// Represents the base class for all plugins.
    /// </summary>
    [NovusPlugin("BASE", "none", "#000000")]
    public abstract class PluginBase : IPluginBase
    {
        /// <summary>
        /// Gets or sets the unique identifier for the plugin.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the settings for the plugin.
        /// </summary>
        public abstract PluginSettings PluginSettings { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginBase"/> class.
        /// </summary>
        public PluginBase()
        {
            WorkTasks = new Dictionary<string, Func<JsonObject, Task<JsonObject>>>();

            // Retrieve the ClassIdAttribute from the Type
            NovusPluginAttribute attribute = (NovusPluginAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(NovusPluginAttribute));
            Id = attribute.Id;
        }

        /// <summary>
        /// Gets or sets the function to save settings asynchronously.
        /// </summary>
        public Func<Task> SaveSettings { get; set; }

        /// <summary>
        /// Gets or sets the callback function to execute JavaScript code.
        /// </summary>
        public Func<string, JsonObject, Task<JsonObject>> ExecuteJavaScriptCodeCallback { get; set; }

        /// <summary>
        /// Gets or sets the function to update the debug log.
        /// </summary>
        public Func<string, JsonObject, Task> UpdateDebugLog { get; set; }

        /// <summary>
        /// Gets or sets the parent node.
        /// </summary>
        public IPluginBase ParentNode { get; set; }

        /// <summary>
        /// Gets or sets the logger instance for the plugin.
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets the type of the UIType component associated with the plugin.
        /// </summary>
        public Type UIType { get; set; }

        /// <summary>
        /// Gets or sets the configuration object.
        /// </summary>
        private object pluginConfig = null;
        public object PluginConfig
        {
            get { return pluginConfig; }
            set
            {
                pluginConfig = value;
                Task.Run(async () =>
                {
                    if (ConfigUpdated != null)
                        await ConfigUpdated().ConfigureAwait(false);
                });
            }
        }

        /// <summary>
        /// Event triggered when the configuration is updated.
        /// </summary>
        protected Func<Task> ConfigUpdated;

        /// <inheritdoc/>
        public IDictionary<string, Func<JsonObject, Task<JsonObject>>> WorkTasks { get; } = new Dictionary<string, Func<JsonObject, Task<JsonObject>>>();

        /// <summary>
        /// Function triggered when the starter node is triggered.
        /// </summary>
        public Func<Task> StarterNodeTriggered = () => { throw new Exception("Only starter nodes are allowed to trigger this"); };

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
        /// <returns></returns>
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

        public abstract Task StopPluginAsync();
    }
}
