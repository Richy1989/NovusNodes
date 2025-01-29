using System.Text.Json;
using Microsoft.Extensions.Logging;
using NovusNodoCore.SaveData;
using NovusNodoCore.Tools;

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
        private readonly NovusModelCreator _novusModelCreator;
        private static readonly SemaphoreSlim _semaphore = new(1, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadSaveManager"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for logging information and errors.</param>
        /// <param name="executionManager">The execution manager responsible for managing nodeModel pages and nodes.</param>
        public LoadSaveManager(ILogger<LoadSaveManager> logger, NovusModelCreator novusModelCreator, ExecutionManager executionManager)
        {
            _logger = logger;
            _novusModelCreator = novusModelCreator;
            _executionManager = executionManager;
            _executionManager.OnProjectChanged += async () =>
            {
                if (_executionManager.IsAutoSaveEnabled)
                    await SaveProject().ConfigureAwait(false);
            };
            _executionManager.OnManualSaveTrigger += ExecutionManager_OnManualSaveTrigger;
        }

        /// <summary>
        /// Handles the manual save trigger event.
        /// </summary>
        private async Task ExecutionManager_OnManualSaveTrigger()
        {
            await SaveProject().ConfigureAwait(false);
        }

        /// <summary>
        /// Saves the current project asynchronously.
        /// </summary>
        /// <param name="pageId">The ID of the page to save.</param>
        private async Task SaveProject()
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            for (int retry = 0; retry < 3; retry++)
            {
                try
                {
                    await SavePages().ConfigureAwait(false);
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
        public async Task SavePages()
        {
            _logger.LogInformation("Saving project...");

            FlowModel flowModel = new();

            foreach (var page in _executionManager.NodePages)
            {
                PageSaveModel pageSaveModel = new()
                {
                    PageId = page.Key,
                    PageName = page.Value.PageName,
                    Nodes = []
                };

                foreach (var node in page.Value.AvailableNodes)
                {
                    _logger.LogDebug($"Saving node: {node.Key}");
                    var nodeSaveModel = await _novusModelCreator.CreateNodeSaveModel(node.Value).ConfigureAwait(false);
                    nodeSaveModel.PageId = page.Key;

                    // Add the node model to the page model
                    pageSaveModel.Nodes.Add(nodeSaveModel);
                }

                flowModel.Pages.Add(pageSaveModel);
            }

            var filePath = Path.Combine(saveDir, $"project.json");

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await JsonSerializer.SerializeAsync(fileStream, flowModel).ConfigureAwait(false);
            }

            _logger.LogInformation("Project saved successfully.");
        }

        /// <summary>
        /// Loads the project asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous load operation.</returns>
        public async Task LoadProject()
        {
            var file = Path.Combine(saveDir, $"project.json");

            if (File.Exists(file))
            {
                _logger.LogDebug($"Loading page file: {file}");
                try
                {
                    FlowModel flowModel = null;
                    using FileStream fileStream = new(file, FileMode.Open, FileAccess.Read, FileShare.Read);
                    {
                        flowModel = await JsonSerializer.DeserializeAsync<FlowModel>(fileStream).ConfigureAwait(false);
                    }
                    if (flowModel != null)
                    {
                        foreach (var pageModel in flowModel.Pages)
                        {
                            await _novusModelCreator.LoadPage(pageModel).ConfigureAwait(false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An error occurred while loading the page file: {file}.");
                }
            }

            if (_executionManager.NodePages.Count == 0)
            {
                await _executionManager.AddNewTab(null, true).ConfigureAwait(false);
            }
        }
    }
}