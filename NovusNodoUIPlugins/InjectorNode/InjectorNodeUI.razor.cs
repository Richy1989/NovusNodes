using Microsoft.Extensions.Logging;

namespace NovusNodoUIPlugins.InjectorNode
{
    /// <summary>
    /// Represents the UI component for the Injector Node.
    /// </summary>
    public partial class InjectorNodeUI
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InjectorNodeUI"/> class.
        /// </summary>
        public InjectorNodeUI()
        {
            Logger?.LogDebug("InjectorNodeUI component initialized");
        }

        /// <summary>
        /// Saves the settings for the Injector Node UI.
        /// </summary>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        public override async Task SaveSettings()
        {
            CreateConfig();
            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Creates the configuration for the Injector Node UI.
        /// </summary>
        public void CreateConfig()
        {
            //Add number sequence to duplicate the variable names
            var nameCounts = new Dictionary<string, int>();
            foreach (var entry in (PluginConfig as InjectorNodeConfig).InjectorEntries)
            {

                if (nameCounts.ContainsKey(entry.Variable))
                {
                    nameCounts[entry.Variable]++;
                    entry.Variable += "_" + nameCounts[entry.Variable].ToString(); // Append the continuous number
                }
                else
                {
                    nameCounts[entry.Variable] = 0; // Initialize the count
                }
            }

            PluginBase.JsonConfig = PluginConfig;
        }

        /// <summary>
        /// Releases the unmanaged resources used by the InjectorNodeUI and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!DisposedValue)
            {
                if (disposing)
                {
                    Logger?.LogDebug("InjectorNodeUI component disposed");
                }
                DisposedValue = true;
            }
        }
    }
}