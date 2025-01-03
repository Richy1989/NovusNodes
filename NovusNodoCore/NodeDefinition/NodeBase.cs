using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NovusNodoPluginLibrary;

namespace NovusNodoCore.NodeDefinition
{
    /// <summary>
    /// Represents the base class for all nodes in the system.
    /// </summary>
    /// <typeparam name="ConfigType">The type of the configuration object.</typeparam>
    public class NodeBase : INodeBase
    {
        private readonly CancellationToken token;
        private readonly SemaphoreSlim semaphoreSlim = new(1, 1);
        private bool isInitialized = false;

        /// <summary>
        /// Gets the plugin base instance.
        /// </summary>
        public IPluginBase PluginBase { get; }

        /// <summary>
        /// Gets or sets the input port of the node.
        /// </summary>
        public InputPort InputPort { get; set; }

        /// <summary>
        /// Gets or sets the output ports of the node.
        /// </summary>
        public Dictionary<string, OutputPort> OutputPorts { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeBase"/> class.
        /// </summary>
        /// <param name="basedPlugin">The plugin base instance.</param>
        /// <param name="token">The cancellation token.</param>
        public NodeBase(IPluginBase basedPlugin, CancellationToken token)
        {
            this.PluginBase = basedPlugin;
            this.token = token;

            InputPort = new InputPort(this);
            OutputPorts = new Dictionary<string, OutputPort>();

            for (int i = 0; i < basedPlugin.WorkTasks.Count; i++)
            {
                var outputPort = new OutputPort(this);
                OutputPorts.Add(outputPort.ID, outputPort);
            }

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
        /// Gets or sets the UI type for the node.
        /// </summary>
        public Type UI { get => PluginBase.UI; set => PluginBase.UI = value; }

        /// <summary>
        /// Gets or sets a value indicating whether the node is enabled.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

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
                        if (!isInitialized)
                        {
                            await PrepareWorkloadAsync().ConfigureAwait(false);
                            isInitialized = true;
                        }

                        int i = 0;
                        // Execute all work tasks from the plugin
                        // Then trigger all the connected nodes from the according output port
                        foreach (var task in PluginBase.WorkTasks.Values)
                        {
                            result = await task(jsonData).ConfigureAwait(false);
                            await TriggerNextNodes(OutputPorts.Values.ElementAt(i), result).ConfigureAwait(false);
                            i++;
                        }
                    }
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
        /// <param name="outputPort">The output port to trigger the next nodes from.</param>
        /// <param name="jsonData">The JSON data to be passed to the next nodes.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task TriggerNextNodes(OutputPort outputPort, string jsonData)
        {
            if (token.IsCancellationRequested) await Task.CompletedTask;

            foreach (var nextNode in outputPort.NextNodes)
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
        public IPluginBase ParentNode { get => PluginBase.ParentNode; set => PluginBase.ParentNode = value; }

        /// <summary>
        /// Gets the type of the node.
        /// </summary>
        public NodeType NodeType => PluginBase.NodeType;

        /// <summary>
        /// Gets the unique identifier for the node.
        /// </summary>
        public string ID { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the JSON configuration for the node.
        /// </summary>
        public string JsonConfig { get => PluginBase.JsonConfig; set => PluginBase.JsonConfig = value; }

        /// <summary>
        /// Gets the name of the node.
        /// </summary>
        public string Name => PluginBase.Name;

        /// <summary>
        /// Gets the background color of the node.
        /// </summary>
        public Color Background => PluginBase.Background;

        /// <summary>
        /// Gets or sets the UI configuration for the node.
        /// </summary>
        public NodeUIConfig UIConfig { get; set; } = new NodeUIConfig();

        /// <summary>
        /// Gets the dictionary of work tasks associated with the node.
        /// </summary>
        public IDictionary<string, Func<string, Task<string>>> WorkTasks => PluginBase.WorkTasks;

        /// <summary>
        /// Prepares the workload asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task PrepareWorkloadAsync()
        {
            await PluginBase.PrepareWorkloadAsync().ConfigureAwait(false);
        }
    }
}
