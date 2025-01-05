using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace NovusNodoUIPlugins.JSFunctionNode
{
    /// <summary>
    /// Represents the UI component for the JavaScript function plugin.
    /// </summary>
    public partial class JavaScriptFunctionPluginUI
    {
        /// <summary>
        /// Saves the settings asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        public override async Task SaveSettings()
        {
            // Get the code from CodeMirror
            PluginConfig = await JS.InvokeAsync<string>("getCodeMirrorValue").ConfigureAwait(false);

            await GetConfig(PluginConfig).ConfigureAwait(false);
            PluginBase.JsonConfig = PluginConfig;
            Logger.LogInformation($"Plugin Info - New JS Code is: \n{PluginConfig}");
        }

        /// <summary>
        /// Method invoked after the component has been rendered.
        /// </summary>
        /// <param name="firstRender">Indicates whether this is the first time the component has been rendered.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // Initialize CodeMirror
                await JS.InvokeVoidAsync("initializeCodeMirror", "CodeEditor", PluginConfig).ConfigureAwait(false);
            }
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
