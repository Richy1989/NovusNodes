using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NovusNodoPluginLibrary;

namespace NovusNodoUIPlugins.NetFunctionNode
{
    public partial class NetFunctionPluginUI
    {
        public override async Task SaveSettings()
        {
            await Task.CompletedTask.ConfigureAwait(false);
        }

        protected override async void Dispose(bool disposing)
        {
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
