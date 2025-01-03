using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace NovusNodoPluginLibrary
{
    public abstract class NovusUIPluginBase : ComponentBase
    {
        /// <summary>
        /// Gets or sets the function to retrieve the configuration.
        /// </summary>
        [Parameter]
        public Func<string, Task> GetConfig { get; set; }

        /// <summary>
        /// Gets or sets the logger instance.
        /// </summary>
        [Parameter]
        public ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets the base plugin instance.
        /// </summary>
        [Parameter]
        public IPluginBase PluginBase { get; set; }

        // To detect redundant calls
        protected bool DisposedValue { get; set; }

        /// <summary>
        /// Public implementation of Dispose pattern callable by consumers.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern.
        /// </summary>
        /// <param name="disposing">Indicates whether the method is called from Dispose.</param>
        protected abstract void Dispose(bool disposing);
    }
}
