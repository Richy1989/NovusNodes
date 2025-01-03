using System.Drawing;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using NovusNodoPluginLibrary;
using NovusNodoPluginLibrary.Helper;

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
        /// Gets the name of the plugin.
        /// </summary>
        public override string Name { get; } = "Debug";

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

            //var message = await PrintVariableRecursive(variables).ConfigureAwait(false);

            foreach (var kvp in jsonData)
            {
                Logger.LogInformation($"{kvp.Key}: {kvp.Value}");
            }

            //Logger.LogInformation(jsonData);
            return await Task.FromResult(new JsonObject()).ConfigureAwait(false);
        }

        private async Task<string> PrintVariableRecursive(Dictionary<string, object> input)
        {
            string message = "";
            foreach (var variable in input)
            {
                if (variable.Value is Dictionary<string, object> nestedVariable)
                {
                    message += $"{variable.Key}";
                    message += await PrintVariableRecursive(nestedVariable).ConfigureAwait(false);
                }
                else
                {
                    message+= $"{variable.Key}: {variable.Value}";
                }
            }
            return await Task.FromResult(message).ConfigureAwait(false);
        }
    }
}
