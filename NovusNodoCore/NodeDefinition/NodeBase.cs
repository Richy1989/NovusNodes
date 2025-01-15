using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using NovusNodoCore.Managers;
using NovusNodoPluginLibrary;

namespace NovusNodoCore.NodeDefinition
{
    /// <summary>
    /// Represents the base class for all nodes in the system.
    /// </summary>
    /// <typeparam name="ConfigType">The type of the configuration object.</typeparam>
    public class NodeBase : INodeBase, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly CancellationToken token;
        private readonly NodeJSEnvironmentManager nodeJSEnvironmentManager;

        // Callback for executing JavaScript code
        public Func<string, JsonObject, Task<JsonObject>> ExecuteJavaScriptCodeCallback { get; set; }

        /// <summary>
        /// Gets the plugin base instance.
        /// </summary>
        public PluginBase PluginBase { get; }

        public NovusPluginAttribute PluginIdAttribute { get; }

        /// <summary>
        /// Gets or sets the logger instance for the plugin.
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets the input port of the node.
        /// </summary>
        public InputPort InputPort { get; set; }

        /// <summary>
        /// Gets or sets the output ports of the node.
        /// </summary>
        public Dictionary<string, OutputPort> OutputPorts { get; set; }

        /// <summary>
        /// Gets or sets the UI type for the node.
        /// </summary>
        public Type UI { get => PluginBase.UI; set => PluginBase.UI = value; }

        /// <summary>Auto Reset event to ensure only one node is executed at a time</summary>
        private readonly AutoResetEvent autoResetEvent = new(true);

        /// <summary>
        /// Gets or sets a value indicating whether the node is enabled.
        /// </summary>
        private bool isEnabled = true;
        public bool IsEnabled
        {
            get
            {
                return isEnabled;
            }
            set
            {
                isEnabled = value;
                OnPropertyChanged();
            }
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
        public string Id { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the JSON configuration for the node.
        /// </summary>
        public object PluginConfig { get => PluginBase.PluginConfig; set => PluginBase.PluginConfig = value; }

        /// <summary>
        /// Gets or sets the name of the node.
        /// </summary>
        private string name;
        public string Name { get { return name; } set { name = value; OnPropertyChanged(); } }

        /// <summary>
        /// Gets or sets the UI configuration for the node.
        /// </summary>
        public NodeUIConfig UIConfig { get; set; } = new NodeUIConfig();

        /// <summary>
        /// Gets the dictionary of work tasks associated with the node.
        /// </summary>
        public IDictionary<string, Func<JsonObject, Task<JsonObject>>> WorkTasks => PluginBase.WorkTasks;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeBase"/> class.
        /// </summary>
        /// <param name="basedPlugin">The plugin base instance.</param>
        /// <param name="Logger">The logger instance.</param>
        /// <param name="nodeJSEnvironmentManager">The NodeJS environment manager instance.</param>
        /// <param name="updateDebugFunction">The function to update the debug log.</param>
        /// <param name="token">The cancellation Token.</param>
        public NodeBase(IPluginBase basedPlugin, NovusPluginAttribute pluginIdAttribute, ILogger<INodeBase> Logger, NodeJSEnvironmentManager nodeJSEnvironmentManager, Func<string, JsonObject, Task> updateDebugFunction, CancellationToken token)
        {
            Name = pluginIdAttribute.Name;
            this.PluginIdAttribute = pluginIdAttribute;
            this.PluginBase = basedPlugin as PluginBase;
            (this.PluginBase as PluginBase).UpdateDebugLog = updateDebugFunction;
            this.Logger = Logger;
            this.token = token;
            this.nodeJSEnvironmentManager = nodeJSEnvironmentManager;
            Init();
        }

        /// <summary>
        /// Initializes the node with the provided plugin base.
        /// </summary>
        public void Init()
        {
            PluginBase.Logger = Logger;
            PluginBase.ExecuteJavaScriptCodeCallback = ExecuteJavaScriptCode;

            InputPort = new InputPort(this);
            OutputPorts = new Dictionary<string, OutputPort>();

            for (int i = 0; i < PluginBase.WorkTasks.Count; i++)
            {
                var outputPort = new OutputPort(this);
                OutputPorts.Add(outputPort.Id, outputPort);
            }

            if (PluginBase.NodeType == NodeType.Starter)
            {
                (PluginBase as PluginBase).StarterNodeTriggered = async () =>
                {
                    if (IsEnabled)
                        await ExecuteNode(new JsonObject()).ConfigureAwait(false);
                };
            }
        }

        /// <summary>
        /// This function is only triggered by starter nodes, when a process should be started.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task TriggerManualExecute()
        {
            await PluginBase.StarterNodeTriggered().ConfigureAwait(false);
        }

        /// <summary>
        /// Gets or sets the function to save settings.
        /// </summary>
        public Func<Task> SaveSettings { get => PluginBase.SaveSettings; set => PluginBase.SaveSettings = value; }

        /// <summary>
        /// Executes the node's workload if the node is enabled, and then triggers the execution of the next nodes.
        /// </summary>
        /// <param name="jsonData">The JSON data to be processed by the node.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task ExecuteNode(JsonObject jsonData)
        {
            autoResetEvent.WaitOne();
            // Execute all work tasks from the plugin
            // Then trigger all the connected nodes from the according output port
            int i = 0;
            foreach (var task in PluginBase.WorkTasks.Values)
            {
                try
                {
                    var result = await task(jsonData).ConfigureAwait(false);

                    if (token.IsCancellationRequested)
                        break;

                    await TriggerNextNodes(OutputPorts.Values.ElementAt(i), result).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error executing work task.");
                }
                i++;
            }
            autoResetEvent.Set();
        }

        /// <summary>
        /// Triggers the execution of the next nodes in the sequence.
        /// </summary>
        /// <param name="outputPort">The output port to trigger the next nodes from.</param>
        /// <param name="jsonData">The JSON data to be passed to the next nodes.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task TriggerNextNodes(OutputPort outputPort, JsonObject jsonData)
        {
            if (token.IsCancellationRequested) await Task.CompletedTask;

            foreach (var nextNode in outputPort.NextNodes)
            {
                nextNode.Value.ParentNode = this;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                if (!token.IsCancellationRequested)
                    nextNode.Value.ExecuteNode(jsonData);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Prepares the workload asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task PrepareWorkloadAsync()
        {
            await PluginBase.PrepareWorkloadAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the provided JavaScript code with the given parameters.
        /// </summary>
        /// <param name="code">The JavaScript code to execute.</param>
        /// <param name="parameters">The parameters to pass to the JavaScript code.</param>
        /// <returns>A task that represents the asynchronous operation, containing the result of the JavaScript execution.</returns>
        public async Task<JsonObject> ExecuteJavaScriptCode(string code, JsonObject parameters)
        {
            int tries = 0;

            while (tries < 3)
            {
                try
                {
                    JsonObject value = nodeJSEnvironmentManager.RunUserCode(code, parameters);
                    return await Task.FromResult(value).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"Error executing JavaScript code. This was try {tries} / 3");
                    tries++;
                    await Task.Delay(50).ConfigureAwait(false);
                }
            }
            Logger.LogError("Failed to execute JavaScript code after 3 tries.");
            return await Task.FromResult(new JsonObject()).ConfigureAwait(false);
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
