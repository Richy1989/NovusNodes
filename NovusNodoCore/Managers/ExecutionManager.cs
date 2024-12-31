using NovusNodoCore.NodeDefinition;
using NovusNodoPluginLibrary;

namespace NovusNodoCore.Managers
{
    /// <summary>
    /// Manages the execution of nodes and plugins.
    /// </summary>
    public class ExecutionManager
    {
        private readonly CancellationTokenSource cts;
        private readonly CancellationToken token;
        private readonly PluginLoader pluginLoader;

        /// <summary>
        /// Event fired when AvailableNodes is updated.
        /// </summary>
        public event Func<INodeBase, Task>? AvailableNodesUpdated;

        /// <summary>
        /// Gets or sets the available plugins.
        /// </summary>
        public IDictionary<string, IPluginBase> AvailablePlugins { get; set; } = new Dictionary<string, IPluginBase>();

        /// <summary>
        /// Gets or sets the available nodes.
        /// </summary>
        public IDictionary<string, INodeBase> AvailableNodes { get; set; } = new Dictionary<string, INodeBase>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionManager"/> class.
        /// </summary>
        public ExecutionManager()
        {
            cts = new CancellationTokenSource();
            token = cts.Token;
            pluginLoader = new PluginLoader(this);
        }

        /// <summary>
        /// Initializes the execution manager by loading plugins and creating test nodes.
        /// </summary>
        public void Initialize()
        {
            pluginLoader.LoadPlugins();
            CreateTestNodes();
        }

        public async Task CreateNode(IPluginBase pluginBase)
        {
            var instance = Activator.CreateInstance(pluginBase.GetType());

            if (instance == null)
            {
                return;
            }

            var plugin = (IPluginBase)instance;

            if (plugin != null)
            {
                INodeBase node = new NodeBase(plugin, token);
                AvailableNodes.Add(node.ID, node);
                await OnAvailableNodesUpdated(node).ConfigureAwait(false);
            }
        }

        private async Task OnAvailableNodesUpdated(INodeBase nodeBase)
        {
            if (AvailableNodesUpdated == null)
            {
                return;
            }
            await AvailableNodesUpdated.Invoke(nodeBase).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates test nodes from the available plugins.
        /// </summary>
        public void CreateTestNodes()
        {
            foreach (var item in AvailablePlugins)
            {
                var instance = Activator.CreateInstance(item.Value.GetType());

                if (instance == null)
                {
                    continue;
                }

                var plugin = (IPluginBase)instance;

                if (plugin != null)
                {
                    NodeBase node = new (plugin, token);
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
