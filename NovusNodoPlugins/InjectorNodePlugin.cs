using System.Drawing;
using System.Text.Json;
using System.Text.Json.Nodes;
using NovusNodoPluginLibrary;
using NovusNodoPluginLibrary.Helper;

namespace NovusNodoPlugins
{
    /// <summary>
    /// Plugin class for injecting nodes with specific configurations and workloads.
    /// </summary>
    public class InjectorNodePlugin : PluginBase
    {
        private TimeSpan interval = TimeSpan.FromMilliseconds(5000);

        /// <summary>
        /// Initializes a new instance of the <see cref="InjectorNodePlugin"/> class.
        /// </summary>
        public InjectorNodePlugin()
        {
            JsonConfig = "{\"Interval\": 1000}";
            AddWorkTask(Workload);
        }

        /// <summary>
        /// Gets the unique identifier for the plugin.
        /// </summary>
        public override string ID { get; } = "0B8A143C-B574-4EBA-BDAF-106B1F279AA2";

        /// <summary>
        /// Gets the name of the plugin.
        /// </summary>
        public override string Name { get; } = "Injector";

        /// <summary>
        /// Gets the background color of the plugin.
        /// </summary>
        public override Color Background { get; } = Color.FromArgb(211, 211, 211);

        /// <summary>
        /// Gets the type of the node.
        /// </summary>
        public override NodeType NodeType { get; } = NodeType.Starter;

        /// <summary>
        /// Prepares the workload asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task PrepareWorkloadAsync()
        {
            var variables = await JsonVariableExtractor.ExtractVariablesAsync(JsonConfig).ConfigureAwait(false);
            // Inline search for the "Interval" variable
            if (variables.TryGetValue("Interval", out var intervalValue))
            {
                Console.WriteLine($"Interval: {intervalValue} ({intervalValue?.GetType().Name})");

                if(intervalValue != null)
                {
                    interval = TimeSpan.FromMilliseconds((double)intervalValue);
                }
            }
        }

        /// <summary>
        /// Defines the workload to be executed by the node.
        /// </summary>
        /// <returns>A function that represents the asynchronous operation.</returns>
        public async Task<JsonObject> Workload(JsonObject jsonData)
        {
            var time = new { DateTime = DateTime.UtcNow.ToString("O") };

            JsonObject jsonObject = JsonSerializer.Deserialize<JsonObject>(JsonSerializer.Serialize(time));

            //var jsonObject = new JsonObject
            //{
            //    ["currentDateTime"] = DateTime.UtcNow.ToString("O")
            //};

            await Task.Delay(interval).ConfigureAwait(false);
            return await Task.FromResult(jsonObject).ConfigureAwait(false);
        }
    }
}