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
            UIType = typeof(SplitConditionUI);

            PluginSettings = new PluginSettings
            {
                //StartIconPath = "Logo_C_sharp.png",
                NodeType = NodeType.Worker,
            };

            PluginConfig = PluginConfig == null ? new SplitConditionConfig() : (SplitConditionConfig)PluginConfig;

            ConfigUpdated += SplitConditionPlugin_ConfigUpdated;
            
            SplitConditionConfig splitConditionConfig = new SplitConditionConfig();
            splitConditionConfig.SplitConditions.Add(Guid.NewGuid().ToString(), new SplitCondition
            {
                Operator = "Equals",
                Value = "1",
                VariablePath = "Variable1",
            });

            //_ = SplitConditionPlugin_ConfigUpdated();

            this.WorkTasks.Add("45F821AD-0D39-4725-A1EB-BB658ED18C79", async (jsonData) =>
            {
                return await Task.FromResult(jsonData).ConfigureAwait(false);
            });

            //AddWorkTask("7C66B0B6-D1E8-4B9B-8FA6-9DCA543ADB59", Workload);
        }

        /// <summary>
        /// Handles the ConfigUpdated event of the SplitConditionPlugin control.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private async Task SplitConditionPlugin_ConfigUpdated()
        {
            WorkTasks.Clear();
            foreach (var condition in ((SplitConditionConfig)PluginConfig).SplitConditions)
            {
                this.WorkTasks.Add(condition.Key, async (jsonData) =>
                {
                    return await Task.FromResult(jsonData).ConfigureAwait(false);
                });
            }
            await RaiseWorkerTasksChangedAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Prepares the workload asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override Task PrepareWorkloadAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Defines the workload to be executed by the node.
        /// </summary>
        /// <param name="jsonData">The JSON data to be processed by the workload.</param>
        /// <returns>A task that represents the asynchronous operation and returns a JSON object result.</returns>
        //public async Task<JsonObject> Workload(JsonObject jsonData)
        //{
        //    PluginConfig = PluginConfig == null ? new SplitConditionConfig() : (SplitConditionConfig)PluginConfig;

        //    return await Task.FromResult(jsonData).ConfigureAwait(false);
        //}

        /// <summary>
        /// Stops the plugin asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task StopPluginAsync()
        {
            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the SplitConditionPlugin and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            ConfigUpdated -= SplitConditionPlugin_ConfigUpdated;
            base.Dispose(disposing);
        }
    }
}
