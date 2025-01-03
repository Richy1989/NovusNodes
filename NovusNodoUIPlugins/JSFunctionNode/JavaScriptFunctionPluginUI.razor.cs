using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using NovusNodoPluginLibrary;

namespace NovusNodoUIPlugins.JSFunctionNode
{
    public partial class JavaScriptFunctionPluginUI
    {
        private string Code { get; set; } = "console.log('Hello, CodeMirror!');";
        /// <summary>
        /// Saves the settings asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        private async Task SaveSettings()
        {
            // Get the code from CodeMirror
            PluginConfig = await JS.InvokeAsync<string>("getCodeMirrorValue");
            //Console.WriteLine($"Saved Code: {Code}");

            await GetConfig(PluginConfig).ConfigureAwait(false);
            PluginBase.JsonConfig = PluginConfig;
            Logger.LogInformation($"Plugin Info - New JS Code is: \n{PluginConfig}");
        }

        protected override async Task OnInitializedAsync()
        {
           // PluginBase.ExecuteJavaScriptCodeCallback = ExecuteJavaScriptCode;
            await base.OnInitializedAsync().ConfigureAwait(false);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // Initialize CodeMirror
                await JS.InvokeVoidAsync("initializeCodeMirror", "CodeEditor", PluginConfig);
            }
        }

        /// <summary>
        /// Executes the provided JavaScript code with the given parameters.
        /// </summary>
        /// <param name="code">The JavaScript code to execute.</param>
        /// <param name="parameters">The parameters to pass to the JavaScript code.</param>
        /// <returns>A task that represents the asynchronous operation, containing the result of the JavaScript execution.</returns>
        public async Task<JsonObject> ExecuteJavaScriptCode(string code, JsonObject parameters)
        {
            JsonObject value = await JS.InvokeAsync<JsonObject>("GJSRunUserCode", [code, parameters]).ConfigureAwait(false);
            return await Task.FromResult(value).ConfigureAwait(false);
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
