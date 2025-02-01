using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using Microsoft.Extensions.Logging;
using NovusNodoPluginLibrary;
using NovusNodoPluginLibrary.Enums;

namespace NovusNodoUIPlugins.InjectorNode
{
    /// <summary>
    /// Plugin class for injecting nodes with specific configurations and workloads.
    /// </summary>
    [NovusPlugin("0B8A143C-B574-4EBA-BDAF-106B1F279AA2", "Inject", "#D3D3D3")]
    public class InjectorNodePlugin : PluginBase
    {
        private CancellationTokenSource _cancellationTokenSource;
        private Task _task;

        /// <summary>
        /// Gets or sets the plugin settings.
        /// </summary>
        public override PluginSettings PluginSettings { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InjectorNodePlugin"/> class.
        /// </summary>
        public InjectorNodePlugin()
        {
            UIType = typeof(InjectorNodeUI);
            AddWorkTask(Workload);

            PluginSettings = new PluginSettings
            {
                NodeType = NodeType.Starter,
                IsManualInjectable = true,
            };

            // Fire and forget, ensuring execution
            // Start the injector plugin task
            _ = Start();

            // Set the callback function to be executed when the config updates. 
            ConfigUpdated = Restart;
        }

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
                Logger.LogError(ex, "Error deserializing JSON object using default inject value");
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
                            return;
                        }

                        await StarterNodeTriggered().ConfigureAwait(false);
                    }
                }
            }, _cancellationTokenSource.Token);
        }

        /// <summary>
        /// Stops the plugin and cancels the running task.
        /// </summary>
        public async Task Stop()
        {
            try
            {
                if (_cancellationTokenSource != null && _task != null)
                {
                    Logger.LogDebug("Stopping injector task!");
                    await _cancellationTokenSource.CancelAsync().ConfigureAwait(false);
                    Logger.LogDebug("Injector task cancellation requested!");
                    await Task.WhenAny(_task, Task.Delay(TimeSpan.FromMicroseconds(500))).ConfigureAwait(false);
                    Logger.LogDebug("Injector task stopped!");

                }
            }
            catch (TaskCanceledException)
            {
                // This exception is thrown when we cancel the current task
            }
            catch (Exception ex)
            {
                Logger.LogDebug(ex, "Error stopping the plugin");
            }
        }

        /// <summary>
        /// Restarts the plugin by stopping and starting it again.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Restart()
        {
            await Stop().ConfigureAwait(false);
            await Start().ConfigureAwait(false);
        }

        /// <summary>
        /// Stops the plugin asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task StopPluginAsync()
        {
            await Stop().ConfigureAwait(false);
        }
    }
}