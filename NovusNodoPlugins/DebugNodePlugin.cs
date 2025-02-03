using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using NovusNodoPluginLibrary;

namespace NovusNodoPlugins
{
    /// <summary>
    /// Plugin class for debugging nodes.
    /// </summary>
    [NovusPlugin("B52F8BEC-BFAA-44D6-9AC2-D2565711A210", "Debug", "#ADD8E6")]
    public class DebugNodePlugin : PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DebugNodePlugin"/> class.
        /// </summary>
        public DebugNodePlugin()
        {
            PluginConfig = "{\"DebugPath\": \"msg.time\"}";

            PluginSettings = new PluginSettings
            {
                NodeType = NodeType.Finisher,
                IsSwitchable = true,
            };

            // Adding the task to the list of tasks
            AddWorkTask("56421658-A6D8-495E-BC08-6B4E368AE03A", Workload);
        }

        /// <summary>
        /// Defines the workload to be executed by the node.
        /// </summary>
        /// <param name="jsonData">The JSON data to be processed by the workload.</param>
        /// <returns>A function representing the asynchronous workload.</returns>
        public async Task<JsonObject> Workload(JsonObject jsonData)
        {
            if (PluginSettings.IsSwitchedOn)
            {
                var message = await PrintVariableRecursive(jsonData).ConfigureAwait(false);

                await UpdateDebugLog.Invoke(Id, jsonData).ConfigureAwait(false);
                Logger.LogInformation(message);
            }
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
            // Recursive function to create a string representation of the JSON object
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

        /// <summary>
        /// Stops the plugin asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task StopPluginAsync()
        {
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
