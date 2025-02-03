using System.Text.Json.Nodes;
using NovusNodoPluginLibrary;

namespace NovusNodoUIPlugins.SplitCondition
{
    /// <summary>
    /// Represents a plugin that executes .NET functions dynamically.
    /// </summary>
    [NovusPlugin("7C66B0B6-D1E8-4B9B-8FA6-9DCA543ADB59", "Split", "#FFF2AF")]
    public class SplitConditionPlugin : PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SplitConditionPlugin"/> class.
        /// </summary>
        public SplitConditionPlugin()
        {
            //UIType = typeof(NetFunctionPluginUI);

            PluginSettings = new PluginSettings
            {
                //StartIconPath = "Logo_C_sharp.png",
                NodeType = NodeType.Worker,
            };

            AddWorkTask("7C66B0B6-D1E8-4B9B-8FA6-9DCA543ADB59", Workload);
        }

        public override Task PrepareWorkloadAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Defines the workload to be executed by the node.
        /// </summary>
        /// <param name="jsonData">The JSON data to be processed by the workload.</param>
        /// <returns>A task that represents the asynchronous operation and returns a JSON object result.</returns>
        public async Task<JsonObject> Workload(JsonObject jsonData)
        {
            //PluginConfig = PluginConfig == null ? new NetFunctionConfig() : (NetFunctionConfig)PluginConfig;

            return await Task.FromResult(jsonData).ConfigureAwait(false);
        }

        public override async Task StopPluginAsync()
        {
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
