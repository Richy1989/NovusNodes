using System.Text.Json;
using Microsoft.Extensions.Logging;
using NovusNodoCore.SaveData;

namespace NovusNodoCore.Managers
{
    /// <summary>
    /// Manages the loading and saving of project data.
    /// </summary>
    public class LoadSaveManager
    {
        private readonly ILogger _logger;
        private readonly string saveDir = Path.Combine(Directory.GetCurrentDirectory(), "projectSaveData");
        private readonly ExecutionManager _executionManager;
        private static readonly SemaphoreSlim _semaphore = new(1, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadSaveManager"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for logging information and errors.</param>
        /// <param name="executionManager">The execution manager responsible for managing nodeModel pages and nodes.</param>
        public LoadSaveManager(ILogger<LoadSaveManager> logger, ExecutionManager executionManager)
        {
            _logger = logger;
            _executionManager = executionManager;
            _executionManager.ProjectChanged += SaveProject;
        }

        /// <summary>
        /// Saves the current project asynchronously.
        /// </summary>
        /// <param name="pageId">The ID of the page to save.</param>
        private async Task SaveProject(string pageId)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            for (int retry = 0; retry < 3; retry++)
            {
                try
                {
                    await SavePage(pageId).ConfigureAwait(false);
                    await _executionManager.AllProjectDataSynced().ConfigureAwait(false);
                    break; // Exit loop if successful
                }
                catch (IOException) when (retry < 2)
                {
                    _logger.LogDebug($"IO Error on saving, at try: {retry}");
                    await Task.Delay(100); // Small delay before retry
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while saving the project.");
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }

        /// <summary>
        /// Saves all nodes in the specified page asynchronously.
        /// </summary>
        /// <param name="pageId">The ID of the page to save.</param>
        /// <returns>A task representing the asynchronous save operation.</returns>
        public async Task SavePage(string pageId)
        {
            _logger.LogInformation("Saving project...");

            var page = _executionManager.NodePages.FirstOrDefault(x => x.Key == pageId);

            PageSaveModel pageSaveModel = new()
            {
                PageId = page.Key,
                PageName = page.Value.PageName,
                Nodes = []
            };

            foreach (var node in page.Value.AvailableNodes)
            {
                _logger.LogDebug($"Saving node: {node.Key}");
                // Create a new nodeModel save model
                var nodeSave = new NodeSaveModel
                {
                    PageId = page.Key,
                    NodeId = node.Key,
                    InputPortId = node.Value.InputPort.Id,
                    PluginBaseId = node.Value.PluginBase.Id,
                    NodeConfig = node.Value.UIConfig.Clone(),
                    ConnectedPorts = []
                };

                // If we have a config type, serialize and add the config
                if (node.Value.PluginIdAttribute.PluginConfigType != null)
                {
                    using var memoryStream = new MemoryStream();
                    await JsonSerializer.SerializeAsync(memoryStream, node.Value.PluginConfig, node.Value.PluginIdAttribute.PluginConfigType).ConfigureAwait(false);
                    memoryStream.Position = 0;
                    using var streamReader = new StreamReader(memoryStream);
                    nodeSave.PluginConfig = await streamReader.ReadToEndAsync().ConfigureAwait(false);
                }
                else
                {
                    //Otherwise, just set the config as a string
                    nodeSave.PluginConfig = node.Value.PluginConfig != null ? (string)node.Value.PluginConfig : null;
                }

                // Add the output ports
                foreach (var outputport in node.Value.OutputPorts)
                {
                    nodeSave.OutputNodes.Add(outputport.Value.Id);
                }

                // Add the connected ports
                foreach (var outputPort in node.Value.InputPort.ConnectedOutputPort.Values)
                {
                    nodeSave.ConnectedPorts.Add(new ConnectionModel { NodeId = outputPort.Node.Id, PortId = outputPort.Id });
                }

                // Add the node model to the page model
                pageSaveModel.Nodes.Add(nodeSave);
            }

            var filePath = Path.Combine(saveDir, $"{page.Key}.json");

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await JsonSerializer.SerializeAsync(fileStream, pageSaveModel).ConfigureAwait(false);
            }

            _logger.LogInformation("Project saved successfully.");
        }

        /// <summary>
        /// Loads the project asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous load operation.</returns>
        public async Task LoadProject()
        {
            List<PageSaveModel> pages = [];
            foreach (var file in Directory.EnumerateFiles(saveDir, "*.json"))
            {
                _logger.LogDebug($"Loading page file: {file}");
                try
                {
                    PageSaveModel pageModel = null;
                    using FileStream fileStream = new(file, FileMode.Open, FileAccess.Read, FileShare.Read);
                    {
                        pageModel = await JsonSerializer.DeserializeAsync<PageSaveModel>(fileStream).ConfigureAwait(false);
                    }
                    if (pageModel != null)
                    {
                        await LoadPage(pageModel).ConfigureAwait(false);
                        pages.Add(pageModel);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An error occurred while loading the page file: {file}.");
                }
            }

            await LoadLinks(pages).ConfigureAwait(false);

            if (_executionManager.NodePages.Count == 0)
            {
                await _executionManager.AddNewTab(null, true).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Loads a specific page asynchronously.
        /// </summary>
        /// <param name="pageModel">The model of the page to load.</param>
        /// <returns>A task representing the asynchronous load operation.</returns>
        public async Task LoadPage(PageSaveModel pageModel)
        {
            _logger.LogInformation("Loading project...");

            var nodePage = await _executionManager.AddNewTab(pageModel.PageId, true).ConfigureAwait(false);

            nodePage.PageName = pageModel.PageName;

            foreach (var nodeModel in pageModel.Nodes)
            {
                if (!_executionManager.AvailablePlugins.ContainsKey(nodeModel.PluginBaseId))
                {
                    _logger.LogError($"Plugin with ID {nodeModel.PluginBaseId} not found.");
                    continue;
                }

                var pluginBaseType = _executionManager.AvailablePlugins[nodeModel.PluginBaseId].Item1;
                var pluginBaseAttribute = _executionManager.AvailablePlugins[nodeModel.PluginBaseId].Item2;

                var node = await nodePage.CreateNode(pluginBaseType, pluginBaseAttribute, nodeModel.NodeId, true).ConfigureAwait(false);
                node.UIConfig.CopyFrom(nodeModel.NodeConfig);

                // I we have a config type, deserialize the config
                if (pluginBaseAttribute.PluginConfigType != null)
                {
                    var config = JsonSerializer.Deserialize(nodeModel.PluginConfig, pluginBaseAttribute.PluginConfigType);
                    config = Convert.ChangeType(config, pluginBaseAttribute.PluginConfigType);
                    node.PluginBase.PluginConfig = config;
                    
                }
                else
                {
                    //Otherwise, just set the config as a string
                    node.PluginBase.PluginConfig = (string)nodeModel.PluginConfig;
                }

                // Create the input port, replace auto created ones
                node.CreateInputPort(nodeModel.InputPortId);

                // Create the output ports, replace auto created ones
                node.OutputPorts.Clear();
                foreach (var outputNodeId in nodeModel.OutputNodes)
                {
                    node.AddOutputPort(outputNodeId);
                }
            }

            _logger.LogInformation("Project loaded successfully.");
        }

        /// <summary>
        /// Loads the links between nodes asynchronously.
        /// </summary>
        /// <param name="pages">The list of pages containing the nodes and their connections.</param>
        /// <returns>A task representing the asynchronous load operation.</returns>
        public async Task LoadLinks(List<PageSaveModel> pages)
        {
            foreach (var page in pages)
            {
                foreach (var nodeModel in page.Nodes)
                {
                    if (nodeModel.ConnectedPorts == null) continue;

                    foreach (var connection in nodeModel.ConnectedPorts)
                    {
                        var node = _executionManager.NodePages[page.PageId].AvailableNodes[nodeModel.NodeId];
                        await _executionManager.NodePages[page.PageId].NewConnection(connection.NodeId, connection.PortId, node.Id, node.InputPort.Id, true).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}