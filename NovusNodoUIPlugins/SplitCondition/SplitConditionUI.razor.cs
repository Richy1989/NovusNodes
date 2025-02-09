using Microsoft.Extensions.Logging;

namespace NovusNodoUIPlugins.SplitCondition
{
    public partial class SplitConditionUI
    {
        /// <summary>
        /// Method invoked when the component is initialized.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            PluginBase.PluginConfig = PluginBase.PluginConfig == null ? new SplitConditionConfig() : (SplitConditionConfig)PluginBase.PluginConfig;
        }

        /// <summary>
        /// Saves the settings for the SplitConditionUI component.
        /// </summary>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        public override async Task SaveSettings()
        {
            SplitConditionConfig splitConditionConfig = (SplitConditionConfig)PluginBase.PluginConfig;
            splitConditionConfig.SplitConditions.Add(Guid.NewGuid().ToString(), new SplitCondition
            {
                Operator = "Equals",
                Value = "1",
                VariablePath = "Variable1",
            });

            await PluginBase.RaiseConfigUpdatedAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the SplitConditionUI component and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!DisposedValue)
            {
                if (disposing)
                {
                    Logger?.LogDebug("Condition Split component disposed");
                }

                DisposedValue = true;
            }
        }
    }
}
