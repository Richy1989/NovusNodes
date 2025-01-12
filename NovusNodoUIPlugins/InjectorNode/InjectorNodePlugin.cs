using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using NovusNodoPluginLibrary;
using NovusNodoPluginLibrary.Enums;

namespace NovusNodoUIPlugins.InjectorNode
{
    /// <summary>
    /// Plugin class for injecting nodes with specific configurations and workloads.
    /// </summary>
    [PluginId("0B8A143C-B574-4EBA-BDAF-106B1F279AA2", "Inject", "#D3D3D3")]
    public class InjectorNodePlugin : PluginBase
    {
        private CancellationTokenSource _cancellationTokenSource;
        private Task _task;

        /// <summary>
        /// Initializes a new instance of the <see cref="InjectorNodePlugin"/> class.
        /// </summary>
        public InjectorNodePlugin()
        {
            UI = typeof(InjectorNodeUI);
            AddWorkTask(Workload);

            Task.Run(async () =>
            {
                await Start().ConfigureAwait(false);
            });

            ConfigUpdated = async () =>
            {
                await Restart().ConfigureAwait(false);
            };
        }

        /// <summary>
        /// Gets the type of the node.
        /// </summary>
        public override NodeType NodeType { get; } = NodeType.Starter;

        /// <summary>
        /// Defines the workload to be executed by the node.
        /// </summary>
        /// <param name="jsonData">The JSON data to process.</param>
        /// <returns>A function that represents the asynchronous operation.</returns>
        public async Task<JsonObject> Workload(JsonObject jsonData)
        {
            StringBuilder jsonConfigBuilder = new();
            InjectorNodeConfig config = PluginConfig == null ? InjectorNodeConfig.CreateDefault() : (InjectorNodeConfig)PluginConfig;

            foreach (var entry in config.InjectorEntries)
            {
                string formattedValue = entry.SelectedType switch
                {
                    PossibleTypesEnum.Number => entry.Value,
                    PossibleTypesEnum.String => $"\"{entry.Value}\"",
                    PossibleTypesEnum.Boolean => bool.Parse(entry.Value).ToString().ToLower(),
                    PossibleTypesEnum.DateTime => $"\"{DateTime.UtcNow.ToString("O")}\"",
                    _ => throw new InvalidOperationException("Unknown type")
                };

                jsonConfigBuilder.Append($"\"{entry.Variable}\":{formattedValue},");
            }

            var json = "{" + jsonConfigBuilder.ToString().TrimEnd(',') + "}";

            JsonObject jsonObject;
            try
            {
                jsonObject = JsonSerializer.Deserialize<JsonObject>(json);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error deserializing JSON object using default check in value");
                var time = new { DateTime = DateTime.UtcNow.ToString("O") };
                string jsonString = JsonSerializer.Serialize(time);
                jsonObject = JsonSerializer.Deserialize<JsonObject>(jsonString);
            }

            return await Task.FromResult(jsonObject).ConfigureAwait(false);
        }

        /// <summary>
        /// Starts the plugin and prepares the workload.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Start()
        {
            InjectorNodeConfig config = PluginConfig == null ? InjectorNodeConfig.CreateDefault() : (InjectorNodeConfig)PluginConfig;

            int injectInterval = (int)(config.InjectIntervalValue * (int)config.InjectInterval);

            await PrepareWorkloadAsync().ConfigureAwait(false);

            if (_task != null && !_task.IsCompleted)
            {
                Logger.LogDebug($"Task is already running.");
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _task = Task.Run(async () =>
            {
                if (config.InjectMode == InjectMode.Interval)
                {
                    while (!_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        await Task.Delay(injectInterval, _cancellationTokenSource.Token).ConfigureAwait(false);

                        if (_cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            break;
                        }

                        await StarterNodeTriggered().ConfigureAwait(false);
                    }
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
                _task.Wait(); // Wait for the task to complete
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