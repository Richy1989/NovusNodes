using System.Drawing;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using NovusNodoPluginLibrary;
using NovusNodoPluginLibrary.Helper;
using NovusNodoUIPlugins.JSFunctionNode;

namespace NovusNodoUIPlugins.InjectorNode
{
    /// <summary>
    /// Plugin class for injecting nodes with specific configurations and workloads.
    /// </summary>
    public class InjectorNodePlugin : PluginBase
    {
        private CancellationTokenSource _cancellationTokenSource;
        private Task _task;
        private TimeSpan _interval = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Initializes a new instance of the <see cref="InjectorNodePlugin"/> class.
        /// </summary>
        public InjectorNodePlugin()
        {
            UI = typeof(InjectorNodeUI);
            JsonConfig = "{\"Interval\": 1000}";
            AddWorkTask(Workload);

            Task.Run(async () =>
            {
                await Start().ConfigureAwait(false);
            });

            ConfigUpdated = async () =>
            {
                await PrepareWorkloadAsync().ConfigureAwait(false);
            };
        }

        /// <summary>
        /// Gets the unique identifier for the plugin.
        /// </summary>
        public override string ID { get; } = "0B8A143C-B574-4EBA-BDAF-106B1F279AA2";

        /// <summary>
        /// Gets or sets the name of the plugin.
        /// </summary>
        public override string Name { get; set; } = "Injector";

        /// <summary>
        /// Gets the background color of the plugin.
        /// </summary>
        public override Color Background { get; } = Color.FromArgb(211, 211, 211);

        /// <summary>
        /// Gets the type of the node.
        /// </summary>
        public override NodeType NodeType { get; } = NodeType.Starter;

        /// <summary>
        /// Prepares the workload asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task PrepareWorkloadAsync()
        {
            var variables = await JsonVariableExtractor.ExtractVariablesAsync(JsonConfig).ConfigureAwait(false);
            // Inline search for the "Interval" variable
            if (variables.TryGetValue("Interval", out var intervalValue))
            {
                Console.WriteLine($"Interval: {intervalValue} ({intervalValue?.GetType().Name})");

                if (intervalValue != null)
                {
                    _interval = TimeSpan.FromMilliseconds((double)intervalValue + 30000);
                }
            }
        }

        /// <summary>
        /// Defines the workload to be executed by the node.
        /// </summary>
        /// <param name="jsonData">The JSON data to process.</param>
        /// <returns>A function that represents the asynchronous operation.</returns>
        public async Task<JsonObject> Workload(JsonObject jsonData)
        {
            var time = new { DateTime = DateTime.UtcNow.ToString("O") };
            JsonObject jsonObject = JsonSerializer.Deserialize<JsonObject>(JsonSerializer.Serialize(time));
            return await Task.FromResult(jsonObject).ConfigureAwait(false);
        }

        /// <summary>
        /// Starts the plugin and prepares the workload.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Start()
        {
            await PrepareWorkloadAsync().ConfigureAwait(false);

            if (_task != null && !_task.IsCompleted)
            {
                Logger.LogDebug($"Task is already running. {ID}");
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _task = Task.Run(async () =>
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    await Task.Delay(_interval, _cancellationTokenSource.Token).ConfigureAwait(false);

                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        break;
                    }

                    await StarterNodeTriggered().ConfigureAwait(false);
                }
            }, _cancellationTokenSource.Token);
        }

        /// <summary>
        /// Stops the plugin and cancels the running task.
        /// </summary>
        public void Stop()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _task.Wait(); // Optionally wait for the task to complete
                _cancellationTokenSource.Dispose();
            }
        }

        /// <summary>
        /// Restarts the plugin by stopping and starting it again.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Restart()
        {
            Stop();
            await Start().ConfigureAwait(false);
        }
    }
}