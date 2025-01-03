using System.Drawing;
using NovusNodoPluginLibrary;

namespace NovusNodoUIPlugins
{
    public class JavaScriptFunctionPlugin : PluginBase
    {
        public JavaScriptFunctionPlugin()
        {
            UI = typeof(JavaScriptFunctionPluginUI);
            JsonConfig = "";
            AddWorkTask(Workload);
        }

        public override string ID => "7BA6BE2A-19A1-44FF-878D-3E408CA17366";

        public override string Name => "JS Function";

        public override Color Background => Color.FromArgb(234, 137, 154);

        public override NodeType NodeType => NodeType.Worker;


        public async Task<string> Workload(string jsonData)
        {
            Console.WriteLine("Hello from JavaScriptFunctionPlugin!");
            return await Task.FromResult(jsonData).ConfigureAwait(false);
        }
    }
}
