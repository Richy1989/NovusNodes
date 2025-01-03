using Microsoft.Extensions.Logging;

namespace NovusNodoUIPlugins.JSFunctionNode
{
    public partial class JavaScriptFunctionPluginUI
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JavaScriptFunctionPluginUI"/> class.
        /// </summary>
        public JavaScriptFunctionPluginUI()
        {
            Console.WriteLine("JavaScriptFunctionPluginUI component initialized");
        }
        

        /// <summary>
        /// Saves the settings asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        private async Task SaveSettings()
        {
            await GetConfig(PluginConfig).ConfigureAwait(false);
            PluginBase.JsonConfig = PluginConfig;
            Logger.LogInformation($"Plugin Info - New JS Code is: \n{PluginConfig}");
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
                    base.Logger?.LogDebug("JavaScriptFunctionPluginUI component disposed");
                }

                DisposedValue = true;
            }
        }
    }
}
