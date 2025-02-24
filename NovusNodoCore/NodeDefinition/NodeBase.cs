﻿using System.ComponentModel;
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
    public class NodeBase : INodeBase
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected readonly CancellationToken BaseToken;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly ExecutionManager executionManager;
        private readonly NodeJSEnvironmentManager nodeJSEnvironmentManager;
        private NodePageManager nodePageManager;
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Gets or sets the callback for executing JavaScript code.
        /// </summary>
        public Func<string, JsonObject, Task<JsonObject>> ExecuteJavaScriptCodeCallback { get; set; }

        /// <summary>Auto Reset event to ensure only one node is executed at a time</summary>
        private readonly AutoResetEvent autoResetEvent = new(true);

        /// <summary>
        /// Gets the plugin base instance.
        /// </summary>
        public PluginBase PluginBase { get; }

        /// <summary>
        /// Gets the plugin ID attribute.
        /// </summary>
        public NovusPluginAttribute PluginIdAttribute { get; }

        /// <summary>
        /// Gets or sets the logger instance for the plugin.
        /// </summary>
        private readonly ILogger<INodeBase> _logger;

        /// <summary>
        /// Gets or sets the input port of the node.
        /// </summary>
        public InputPort InputPort { get; set; }

        /// <summary>
        /// Gets or sets the output ports of the node.
        /// </summary>
        public Dictionary<string, OutputPort> OutputPorts { get; set; }

        /// <summary>
        /// Gets or sets the UIType type for the node.
        /// </summary>
        public Type UIType { get => PluginBase.UIType; set => PluginBase.UIType = value; }

        /// <summary>
        /// Gets or sets the parent node.
        /// </summary>
        public IPluginBase ParentNode { get => PluginBase.ParentNode; set => PluginBase.ParentNode = value; }

        /// <summary>
        /// Gets or sets the plugin settings.
        /// </summary>
        public PluginSettings PluginSettings { get => PluginBase.PluginSettings; set => PluginBase.PluginSettings = value; }

        /// <summary>
        /// Gets the unique identifier for the node.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets or sets the JSON configuration for the node.
        /// </summary>
        public object PluginConfig
        {
            get => PluginBase.PluginConfig;
            set
            {
                PluginBase.PluginConfig = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the node.
        /// </summary>
        public string Name { get { return UIConfig.Name; } set { UIConfig.Name = value; } }

        /// <summary>
        /// Gets or sets the UIType configuration for the node.
        /// </summary>
        public NodeUIConfig UIConfig { get; set; }

        /// <summary>
        /// Gets the dictionary of work tasks associated with the node.
        /// </summary>
        public IDictionary<string, Func<JsonObject, Task<JsonObject>>> WorkTasks => PluginBase.WorkTasks;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeBase"/> class.
        /// </summary>
        /// <param name="loggerFactory">The logger factory instance.</param>
        /// <param name="id">The unique identifier for the node.</param>
        /// <param name="basedPlugin">The base plugin instance.</param>
        /// <param name="UIConfig">The UI configuration for the node.</param>
        /// <param name="executionManager">The execution manager instance.</param>
        /// <param name="pluginIdAttribute">The plugin ID attribute.</param>
        /// <param name="nodePageManager">The node page manager instance.</param>
        /// <param name="_nodeJSEnvironmentManager">The NodeJS environment manager instance.</param>
        /// <param name="updateDebugFunction">The function to update the debug log.</param>
        public NodeBase(
            ILoggerFactory loggerFactory,
            string id,
            IPluginBase basedPlugin,
            NodeUIConfig UIConfig,
            ExecutionManager executionManager,
            NovusPluginAttribute pluginIdAttribute,
            NodePageManager nodePageManager,
            NodeJSEnvironmentManager _nodeJSEnvironmentManager,
            Func<string, JsonObject, Task> updateDebugFunction)
        {
            _logger = loggerFactory.CreateLogger<INodeBase>();
            _loggerFactory = loggerFactory;
            this.UIConfig = UIConfig;
            // Generate a new ID if none is provided, it is provided by load project
            Id = id ?? Guid.NewGuid().ToString();

            UIConfig.PropertyChanged += (sender, e) => OnPropertyChanged(e.PropertyName);

            this.nodePageManager = nodePageManager;
            this.executionManager = executionManager;
            Name = pluginIdAttribute.Name;
            PluginIdAttribute = pluginIdAttribute;
            PluginBase = basedPlugin as PluginBase;
            PluginBase.UpdateDebugLog = updateDebugFunction;

            this.nodeJSEnvironmentManager = _nodeJSEnvironmentManager;

            // Create a new cancellation token source
            cancellationTokenSource = new CancellationTokenSource();
            BaseToken = cancellationTokenSource.Token;

            PluginBase.OnWorkerTasksChanged += PluginBase_OnWorkerTasksChanged;

            Init();
        }

        private async Task PluginBase_OnWorkerTasksChanged()
        {
            await nodePageManager.PortsChanged(this.Id).ConfigureAwait(false);
        }

        /// <summary>
        /// Initializes the node with the provided plugin base.
        /// </summary>
        /// <param name="createPorts">Indicates whether to create ports for the node.</param>
        private void Init()
        {
            // Set the start icon paths
            if (PluginSettings.StartIconPath != null)
            {
                PluginSettings.StartIconPath = Path.Combine("pluginicons", this.PluginIdAttribute.AssemblyName, PluginSettings.StartIconPath);
            }

            // Set the end icon paths
            if (PluginSettings.EndIconPath != null)
            {
                PluginSettings.EndIconPath = Path.Combine("pluginicons", this.PluginIdAttribute.AssemblyName, PluginSettings.EndIconPath);
            }

            PluginBase.Logger = _loggerFactory.CreateLogger(PluginBase.GetType());
            PluginBase.ExecuteJavaScriptCodeCallback = ExecuteJavaScriptCode;

            CreatePorts();

            if (PluginSettings.NodeType == NodeType.Starter)
            {
                PluginBase.StarterNodeTriggered = async () =>
                {
                    if (UIConfig.IsEnabled && executionManager.IsExecutionAllowed)
                        await ExecuteNode([]).ConfigureAwait(false);
                };
            }
        }

        /// <summary>
        /// Creates the input and output ports for the node.
        /// </summary>
        public void CreatePorts()
        {
            if (PluginSettings.NodeType != NodeType.Starter)
                CreateInputPort();

            AddOutputPorts();
        }

        /// <summary>
        /// Creates the input port for the node.
        /// </summary>
        /// <param name="id">The unique identifier for the input port.</param>
        public void CreateInputPort(string id = null)
        {
            InputPort = new InputPort(this);
            if (id != null)
                InputPort.Id = id;
        }

        /// <summary>
        /// Adds the output ports to the node. 
        /// </summary>
        public void AddOutputPorts()
        {
            OutputPorts = [];
            if (PluginSettings.NodeType != NodeType.Finisher)
            {
                foreach (var worker in PluginBase.WorkTasks)
                {
                    AddOutputPort(null, worker.Key);
                }
            }
        }

        /// <summary>
        /// Adds an output port to the node.
        /// </summary>
        public void AddOutputPort(string id, string relatedWorkerTask)
        {
            var outputPort = new OutputPort(this)
            {
                RelatedWorkerTaskId = relatedWorkerTask
            };

            //If null we use the auto created one from the Port
            if(id != null)
                outputPort.Id = id;

            OutputPorts.Add(outputPort.Id, outputPort);
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
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task SaveSettings()
        {
            //Trigger the plugin to save its settings
            await PluginBase.SaveSettings().ConfigureAwait(false);

            //Notify that the project has changed!
            await executionManager.RaiseProjectChangedAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Executes the node's workload if the node is enabled, and then triggers the execution of the next nodes.
        /// </summary>
        /// <param name="jsonData">The JSON data to be processed by the node.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task ExecuteNode(JsonObject jsonData)
        {
            if (!UIConfig.IsEnabled) return;

            autoResetEvent.WaitOne();

            // Execute all work tasks from the plugin
            // Then trigger all the connected nodes from the according output port
            foreach (var task in PluginBase.WorkTasks)
            {
                try
                {
                    var result = await task.Value(jsonData).ConfigureAwait(false);

                    if (BaseToken.IsCancellationRequested)
                        break;

                    string workerTaskId = task.Key;
                    var relatedOutputPort = OutputPorts.Values.FirstOrDefault(x => x.RelatedWorkerTaskId == workerTaskId);

                    if (relatedOutputPort != null)
                    {
                        await TriggerNextNodes(relatedOutputPort, result).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing work task.");
                }
            }
            autoResetEvent.Set();
        }

        /// <summary>Triggers the execution of the next nodes in the sequence.</summary>
        /// <param name="outputPort">The output port to trigger the next nodes from.</param>
        /// <param name="jsonData">The JSON data to be passed to the next nodes.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task TriggerNextNodes(OutputPort outputPort, JsonObject jsonData)
        {
            if (BaseToken.IsCancellationRequested) await Task.CompletedTask;

            foreach (var nextNode in outputPort.NextNodes)
            {
                // Clone the JsonObject / otherwise we have references with will influence each other
                var clone = JsonNode.Parse(jsonData.ToJsonString()).AsObject();

                nextNode.Value.ParentNode = this;
                if (!BaseToken.IsCancellationRequested)
                    _ = nextNode.Value.ExecuteNode(clone);
            }
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
            try
            {
                JsonObject value = nodeJSEnvironmentManager.RunUserCode(code, parameters);
                return await Task.FromResult(value).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing JavaScript code.");
            }

            return await Task.FromResult(new JsonObject()).ConfigureAwait(false);
        }

        /// <summary>
        /// Closes the node asynchronously, stopping the plugin and canceling any ongoing operations.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task CloseNodeAsync()
        {
            await PluginBase.StopPluginAsync().ConfigureAwait(false);
            await cancellationTokenSource.CancelAsync().ConfigureAwait(false);
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
