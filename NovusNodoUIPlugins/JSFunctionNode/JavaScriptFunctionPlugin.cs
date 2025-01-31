using System.Text.Json.Nodes;
using NovusNodoPluginLibrary;

namespace NovusNodoUIPlugins.JSFunctionNode
{
    /// <summary>
    /// Represents a plugin that executes JavaScript functions.
    /// </summary>
    [NovusPlugin("7BA6BE2A-19A1-44FF-878D-3E408CA17366", "JS Function", "#ea899a")]
    public class JavaScriptFunctionPlugin : PluginBase
    {
        /// <summary>
        /// Gets or sets the plugin configuration.
        /// </summary>
        public override PluginSettings PluginSettings { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptFunctionPlugin"/> class.
        /// </summary>
        public JavaScriptFunctionPlugin()
        {
            UIType = typeof(JavaScriptFunctionPluginUI);
            PluginSettings = new PluginSettings
            {
                StartIconPath = "jslogo.png",
                NodeType = NodeType.Worker,
            };

            PluginConfig = "return msg;";
            AddWorkTask(Workload);
        }

        /// <summary>
        /// Defines the workload to be executed by the node.
        /// </summary>
        /// <param name="jsonData">The JSON data to be processed by the workload.</param>
        /// <returns>A task that represents the asynchronous operation and returns a string result.</returns>
        public async Task<JsonObject> Workload(JsonObject jsonData)
        {
            // Execute the JavaScript code and return the result
            //The config is the JavaScript code to be executed
            return await ExecuteJavaScriptCodeCallback((string)PluginConfig, jsonData).ConfigureAwait(false);
        }

        public override async Task StopPluginAsync()
        {
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
