﻿using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;

namespace NovusNodoPluginLibrary
{
    /// <summary>
    /// Represents the base class for all plugins.
    /// </summary>
    [NovusPlugin("BASE", "none", "#000000")]
    public abstract class PluginBase : IPluginBase, IDisposable
    {
        private bool _disposedValue = false;

        /// <summary>
        /// Gets or sets the unique identifier for the plugin.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the settings for the plugin.
        /// </summary>
        public PluginSettings PluginSettings { get; set; }

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
        /// Event triggered when the worker tasks are changed.
        /// </summary>
        public event Func<Task> OnWorkerTasksChanged;

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
                _ = RaiseConfigUpdatedAsync();
            }
        }

        /// <summary>
        /// Event triggered when the configuration is updated.
        /// </summary>
        protected event Func<Task> ConfigUpdated;

        /// <inheritdoc/>
        public IDictionary<string, Func<JsonObject, Task<JsonObject>>> WorkTasks { get; } = new Dictionary<string, Func<JsonObject, Task<JsonObject>>>();

        /// <summary>
        /// Function triggered when the starter node is triggered.
        /// </summary>
        public Func<Task> StarterNodeTriggered = () => { throw new Exception("Only starter nodes are allowed to trigger this"); };

        /// <summary>
        /// Prepares the workload asynchronously.
        /// </summary>
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

        /// <summary>
        /// Starts the plugin asynchronously.
        /// </summary>
        public abstract Task StopPluginAsync();

        /// <summary>
        /// Raises the configuration updated event asynchronously.
        /// </summary>
        public async Task RaiseConfigUpdatedAsync()
        {
            var handlers = ConfigUpdated?.GetInvocationList(); // Get all subscribers
            if (handlers == null) return;

            foreach (var handler in handlers.Cast<Func<Task>>())
            {
                await handler().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Raises the worker tasks changed event asynchronously.
        /// </summary>
        public async Task RaiseWorkerTasksChangedAsync()
        {
            var handlers = OnWorkerTasksChanged?.GetInvocationList(); // Get all subscribers
            if (handlers == null) return;

            foreach (var handler in handlers.Cast<Func<Task>>())
            {
                await handler().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Public implementation of Dispose pattern callable by consumers.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern.
        /// </summary>
        /// <param name="disposing">Indicates whether the method is called from Dispose.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Logger.LogDebug($"Disposing PluginBase with");
                }

                _disposedValue = true;
            }
        }
    }
}
