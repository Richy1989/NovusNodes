using Microsoft.Extensions.Logging;

namespace NovusNodoUIPlugins.JSFunctionNode
{
    /// <summary>
    /// Represents the UI component for the JavaScript function plugin.
    /// </summary>
    public partial class JavaScriptFunctionPluginUI
    {
        string text = "";
        /// <summary>
        /// Saves the settings asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        public override async Task SaveSettings()
        {
            PluginBase.JsonConfig = PluginConfig;
            Logger.LogInformation($"Plugin Info - New JS Code is: \n{PluginConfig}");
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
                    Logger?.LogDebug("JavaScriptFunctionPluginUI component disposed");
                }

                DisposedValue = true;
            }
        }
    }
}
