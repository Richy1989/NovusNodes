using Microsoft.Extensions.Logging;

namespace NovusNodoUIPlugins.InjectorNode
{
    public partial class InjectorNodeUI
    {
        public InjectorNodeUI()
        {
            Logger?.LogDebug("InjectorNodeUI component initialized");
        }

        public override async Task SaveSettings()
        {
            CreateConfig();
            await Task.CompletedTask.ConfigureAwait(false);
        }

        public void CreateConfig()
        {
            //InjectorNodeConfig injectorNodeConfig = new InjectorNodeConfig
            //{
            //    InjectInterval = InjectInterval,
            //    InjectIntervalValue = InjectIntervalValue,
            //    InjectMode = InjectMode,
            //    InjectorEntries = Entries.ToList()
            //};

            PluginBase.JsonConfig = (object)PluginConfig;
        }

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