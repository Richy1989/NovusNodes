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

        /// <inheritdoc/>
        public Func<Task> SaveSettings { get; set; }

        /// <inheritdoc/>
        public Func<string, JsonObject, Task<JsonObject>> ExecuteJavaScriptCodeCallback { get; set; }

        /// <inheritdoc/>
        public Func<string, JsonObject, Task> UpdateDebugLog { get; set; }

        /// <inheritdoc/>
        public IPluginBase ParentNode { get; set; }

        /// <inheritdoc/>
        public ILogger Logger { get; set; }

        /// <inheritdoc/>
        public Type UI { get; set; }

        /// <inheritdoc/>
        public abstract string ID { get; }

        /// <inheritdoc/>
        public abstract string Name { get; set; }

        /// <inheritdoc/>
        public abstract Color Background { get; }

        /// <inheritdoc/>
        public abstract NodeType NodeType { get; }

        private string jsonConfig = "";
        /// <inheritdoc/>
        public string JsonConfig
        {
            get { return jsonConfig; }
            set
            {
                jsonConfig = value;
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

        /// <inheritdoc/>
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
