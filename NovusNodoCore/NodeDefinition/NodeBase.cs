using System.Drawing;
using NovusNodoPluginLibrary;

namespace NovusNodoCore.NodeDefinition
{
    /// <summary>
    /// Represents the base class for all nodes in the system.
    /// </summary>
    /// <typeparam name="ConfigType">The type of the configuration object.</typeparam>
    /// <remarks>
    /// Initializes a new instance of the <see cref="NodeBase{ConfigType}"/> class.
    /// </remarks>
    /// <param name="basedPlugin">The plugin base instance.</param>
    /// <param name="token">The cancellation token.</param>
    public class NodeBase : INodeBase
    {
        private readonly IPluginBase basedPlugin;
        private readonly CancellationToken token;
        private readonly SemaphoreSlim semaphoreSlim = new(1, 1);
        private bool isInitialized = false;

        /// <summary>
        /// Gets or sets the input port of the node.
        /// </summary>
        public NodePort InputPort { get; set; } = new NodePort(true);
        /// <summary>
        /// Gets or sets the output port of the node.
        /// </summary>
        public NodePort OutputPort { get; set; } = new NodePort(false);

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeBase"/> class.
        /// </summary>
        /// <param name="basedPlugin">The plugin base instance.</param>
        /// <param name="token">The cancellation token.</param>
        public NodeBase(IPluginBase basedPlugin, CancellationToken token)
        {
            this.basedPlugin = basedPlugin;
            this.token = token;

            if (basedPlugin.NodeType == NodeType.Starter)
            {
                Task executor = new(async () =>
                {
                    await ExecuteNode(string.Empty).ConfigureAwait(false);
                });
                executor.Start();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the node is enabled.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets the dictionary of next nodes.
        /// </summary>
        public IDictionary<string, INodeBase> NextNodes { get; set; } = new Dictionary<string, INodeBase>();

        /// <summary>
        /// Executes the node's workload if the node is enabled, and then triggers the execution of the next nodes.
        /// </summary>
        /// <param name="jsonData">The JSON data to be processed by the node.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task ExecuteNode(string jsonData)
        {
            await semaphoreSlim.WaitAsync().ConfigureAwait(false);

            try
            {
                bool run = true;
                while (run && !token.IsCancellationRequested)
                {
                    if (NodeType != NodeType.Starter)
                    {
                        run = false;
                    }

                    string result = string.Empty;

                    if (IsEnabled)
                    {
                        if(!isInitialized)
                        {
                            await PrepareWorkloadAsync().ConfigureAwait(false);
                            isInitialized = true;
                        }

                        result = await Workload(jsonData).Invoke().ConfigureAwait(false);
                    }

                    // Not enabled starter nodes should not trigger next nodes
                    if (NodeType == NodeType.Starter && !IsEnabled) return;

                    await TriggerNextNodes(result).ConfigureAwait(false);
                }
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        /// <summary>
        /// Triggers the execution of the next nodes in the sequence.
        /// </summary>
        /// <param name="jsonData">The JSON data to be passed to the next nodes.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task TriggerNextNodes(string jsonData)
        {
            if (token.IsCancellationRequested) return;

            foreach (var nextNode in NextNodes)
            {
                nextNode.Value.ParentNode = this;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                nextNode.Value.ExecuteNode(jsonData);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Gets or sets the parent node.
        /// </summary>
        public IPluginBase ParentNode { get => basedPlugin.ParentNode; set => basedPlugin.ParentNode = value; }

        /// <summary>
        /// Gets the type of the node.
        /// </summary>
        public NodeType NodeType => basedPlugin.NodeType;

        /// <summary>
        /// Gets the unique identifier for the node.
        /// </summary>
        public string ID { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the JSON configuration for the node.
        /// </summary>
        public string JsonConfig { get => basedPlugin.JsonConfig; set => basedPlugin.JsonConfig = value; }

        /// <summary>
        /// Gets the name of the node.
        /// </summary>
        public string Name => basedPlugin.Name;

        /// <summary>
        /// Gets the background color of the node.
        /// </summary>
        public Color Background => basedPlugin.Background;

        public NodeUIConfig UIConfig { get; set; } = new NodeUIConfig();
      
        /// <summary>
        /// Defines the workload to be executed by the node.
        /// </summary>
        /// <param name="jsonData">The JSON data to be processed by the workload.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Func<Task<string>> Workload(string jsonData)
        {
            return basedPlugin.Workload(jsonData);
        }

        /// <summary>
        /// Prepares the workload asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task PrepareWorkloadAsync()
        {
            await basedPlugin.PrepareWorkloadAsync().ConfigureAwait(false);
        }
    }
}
