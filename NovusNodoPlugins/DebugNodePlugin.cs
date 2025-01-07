using System.Drawing;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using NovusNodoPluginLibrary;

namespace NovusNodoPlugins
{
    /// <summary>
    /// Plugin class for debugging nodes.
    /// </summary>
    public class DebugNodePlugin : PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DebugNodePlugin"/> class.
        /// </summary>
        public DebugNodePlugin()
        {
            JsonConfig = "{\"DebugPath\": \"msg.time\"}";

            //Adding the task to the list of tasks
            base.AddWorkTask(Workload);
        }

        /// <summary>
        /// Gets the unique identifier for the plugin.
        /// </summary>
        public override string ID { get; } = "B52F8BEC-BFAA-44D6-9AC2-D2565711A210";

        /// <summary>
        /// Gets or sets the name of the plugin.
        /// </summary>
        public override string Name { get; set; } = "Debug";

        /// <summary>
        /// Gets the background color of the plugin.
        /// </summary>
        public override Color Background { get; } = Color.FromArgb(173, 216, 230);

        /// <summary>
        /// Gets the type of the node.
        /// </summary>
        public override NodeType NodeType { get; } = NodeType.Finisher;

        /// <summary>
        /// Defines the workload to be executed by the node.
        /// </summary>
        /// <param name="jsonData">The JSON data to be processed by the workload.</param>
        /// <returns>A function representing the asynchronous workload.</returns>
        public async Task<JsonObject> Workload(JsonObject jsonData)
        {
            //var variables = await JsonVariableExtractor.ExtractVariablesAsync(jsonData).ConfigureAwait(false);

            var message = await PrintVariableRecursive(jsonData).ConfigureAwait(false);

            //foreach (var kvp in jsonData)
            //{
            //    Logger.LogInformation($"{kvp.Key}: {kvp.Value}");
            //}
            await UpdateDebugLog.Invoke(ID, jsonData).ConfigureAwait(false);
            Logger.LogInformation(message);
            return await Task.FromResult(new JsonObject()).ConfigureAwait(false);
        }

        /// <summary>
        /// Recursively prints the variables in the JSON object.
        /// </summary>
        /// <param name="jsonObject">The JSON object to be printed.</param>
        /// <returns>A string representation of the JSON object.</returns>
        private async Task<string> PrintVariableRecursive(JsonObject jsonObject)
        {
            if (jsonObject == null)
            {
                return "";
            }

            string message = "";
            //recursive function to create a string representation of the JSON object
            foreach (var kvp in jsonObject)
            {
                if (kvp.Value is JsonObject)
                {
                    message += $"{kvp.Key}: {await PrintVariableRecursive(kvp.Value as JsonObject).ConfigureAwait(false)}\n";
                }
                else
                {
                    message += $"{kvp.Key}: {kvp.Value}\n";
                }
            }
            return message;
        }
    }
}
