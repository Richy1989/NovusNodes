using Microsoft.Extensions.Logging;

namespace NovusNodoUIPlugins.NetFunctionNode
{
    public partial class NetFunctionPluginUI
    {
        /// <summary>
        /// Saves the settings asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        public override async Task SaveSettings()
        {
            Logger.LogDebug($"Plugin Info - New .NET Code is: \n{PluginBase.PluginConfig}");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern.
        /// </summary>
        /// <param name="disposing">Indicates whether the method is called from Dispose.</param>
        protected override void Dispose(bool disposing)
        {
            if (!DisposedValue)
            {
                if (disposing)
                {
                    Logger?.LogDebug(".NET Function component disposed");
                }

                DisposedValue = true;
            }
        }
    }
}
