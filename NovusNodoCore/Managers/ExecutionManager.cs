using NovusNodoCore.NodeDefinition;
using NovusNodoPluginLibrary;

namespace NovusNodoCore.Managers
{
    public class ExecutionManager
    {
        private readonly CancellationTokenSource cts;
        private readonly CancellationToken token;
        private readonly PluginLoader pluginLoader;

        public ExecutionManager()
        {
            cts = new CancellationTokenSource();
            token = cts.Token;
            pluginLoader = new PluginLoader(this);
        }

        public void Initialize()
        {
            pluginLoader.LoadPlugins();
            CreateTestNodes();
        }

        public IDictionary<string, Type> AvailablePlugins { get; set; } = new Dictionary<string, Type>();

        public IDictionary<string, INodeBase> AvailableNodes { get; set; } = new Dictionary<string, INodeBase> ();

        public void CreateTestNodes()
        {
            foreach (var item in AvailablePlugins)
            {
                var instance = Activator.CreateInstance(item.Value);

                if (instance == null)
                {
                    continue;
                }

                var plugin = (IPluginBase)instance;

                if (plugin != null)
                {
                    INodeBase node = new NodeBase(plugin, token);
                    AvailableNodes.Add(node.ID, node);
                }
            }

            INodeBase? starter = null;
            foreach (var node in AvailableNodes)
            {
                if (node.Value.NodeType == NodeType.Starter)
                {
                    starter = node.Value;
                }
            }

            foreach (var node in AvailableNodes)
            {
                if (node.Value.NodeType != NodeType.Starter)
                {
                    starter.NextNodes.Add(node.Value.ID, node.Value);
                }
            }


        }
    }
}
