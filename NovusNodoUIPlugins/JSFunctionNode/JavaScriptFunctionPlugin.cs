using System.Drawing;
using System.Text.Json.Nodes;
using NovusNodoPluginLibrary;

namespace NovusNodoUIPlugins.JSFunctionNode
{
    /// <summary>
    /// Represents a plugin that executes JavaScript functions.
    /// </summary>
    public class JavaScriptFunctionPlugin : PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptFunctionPlugin"/> class.
        /// </summary>
        public JavaScriptFunctionPlugin()
        {
            UI = typeof(JavaScriptFunctionPluginUI);
            JsonConfig = "return msg;";
            AddWorkTask(Workload);
        }

        /// <summary>
        /// Gets the unique identifier for the plugin.
        /// </summary>
        public override string ID => "7BA6BE2A-19A1-44FF-878D-3E408CA17366";

        /// <summary>
        /// Gets or sets the name of the plugin.
        /// </summary>
        public override string Name { get; set; } = "JS Function";

        /// <summary>
        /// Gets the background color of the plugin.
        /// </summary>
        public override Color Background => Color.FromArgb(234, 137, 154);

        /// <summary>
        /// Gets the type of the node.
        /// </summary>
        public override NodeType NodeType => NodeType.Worker;

        /// <summary>
        /// Defines the workload to be executed by the node.
        /// </summary>
        /// <param name="jsonData">The JSON data to be processed by the workload.</param>
        /// <returns>A task that represents the asynchronous operation and returns a string result.</returns>
        public async Task<JsonObject> Workload(JsonObject jsonData)
        {
            //JsonConfig is the written JS Program at this point
            return await ExecuteJavaScriptCodeCallback(JsonConfig, jsonData).ConfigureAwait(false);
        }
    }
}
