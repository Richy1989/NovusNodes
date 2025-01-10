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
            base.PluginBase.JsonConfig = "";
            await Task.CompletedTask.ConfigureAwait(false);
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

        public void CreateConfig()
        {
            foreach (var entry in Entries)
            {
                //if(entry.SelectedType)
                //base.PluginBase.JsonConfig += entry.Key + ":" + entry.Value + ",";
            }
        }
    }
}