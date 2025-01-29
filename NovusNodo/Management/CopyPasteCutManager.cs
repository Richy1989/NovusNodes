using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NovusNodoCore.Managers;
using NovusNodoCore.NodeDefinition;
using NovusNodoCore.SaveData;
using NovusNodoCore.Tools;

namespace NovusNodo.Management
{
    /// <summary>
    /// Manages copy, paste, and cut operations for nodes.
    /// </summary>
    public class CopyPasteCutManager
    {
        private readonly NovusModelCreator _novusModelCreator;
        private readonly ILogger<CopyPasteCutManager> _logger;
        private readonly ExecutionManager _executionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyPasteCutManager"/> class.
        /// </summary>
        /// <param name="logger">The logger instance for logging information and errors.</param>
        /// <param name="novusModelCreator">The model creator for creating node save models.</param>
        /// <param name="executionManager">The execution manager for managing node pages.</param>
        public CopyPasteCutManager(ILogger<CopyPasteCutManager> logger, NovusModelCreator novusModelCreator, ExecutionManager executionManager)
        {
            _novusModelCreator = novusModelCreator;
            _executionManager = executionManager;
            _logger = logger;
        }

        /// <summary>
        /// Handles the copy operation for a list of nodes.
        /// </summary>
        /// <param name="nodeBases">The list of nodes to copy.</param>
        /// <returns>A JSON string representing the copied nodes.</returns>
        public async Task<string> HandleCopy(List<NodeBase> nodeBases)
        {
            List<NodeSaveModel> modelList = new();
            foreach (NodeBase node in nodeBases)
            {
                var nodeModel = await _novusModelCreator.CreateNodeSaveModel(node).ConfigureAwait(false);
                modelList.Add(nodeModel);
            }

            using var memoryStream = new MemoryStream();
            await JsonSerializer.SerializeAsync(memoryStream, modelList).ConfigureAwait(false);
            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }

        /// <summary>
        /// Handles the paste operation for a list of nodes.
        /// </summary>
        /// <param name="json">The JSON string representing the nodes to paste.</param>
        /// <param name="pageId">The ID of the page to paste the nodes into.</param>
        /// <param name="mouseX">The X coordinate of the mouse position.</param>
        /// <param name="mouseY">The Y coordinate of the mouse position.</param>
        /// <param name="translateIds">Indicates whether to translate the IDs of the nodes.</param>
        public async Task<List<string>> HandlePaste(string json, string pageId, double mouseX, double mouseY, bool translateIds)
        {
            List<NodeSaveModel> modelList = null;

            // Check if the page exists
            if (!_executionManager.NodePages.ContainsKey(pageId))
            {
                return [];
            }

            // Deserialize the copied nodes if Clipboard matches JSON format
            try
            {
                using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(json));
                modelList = await JsonSerializer.DeserializeAsync<List<NodeSaveModel>>(memoryStream).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Failed to deserialize copied nodes.");
                return [];
            }

            // Check if the list of node models is available
            if (modelList == null || modelList.Count < 1)
            {
                return [];
            }

            // Translate the modelList to an ID translated modelList
            if (translateIds)
            {
                _novusModelCreator.CreateIdTranslatedModel(modelList);
            }

            // Load the nodes
            var page = _executionManager.NodePages[pageId];

            //Move the position of the pasted nodes slightly
            foreach (var node in modelList)
            {
                node.NodeConfig.X += 50;
                node.NodeConfig.Y += 50;
            }

            _logger.LogDebug($"Creating pasted nodes to page {pageId}.");
            await _novusModelCreator.LoadNodes(modelList, page).ConfigureAwait(false);

            _logger.LogDebug($"Creating links for pasted nodes.");
            await _novusModelCreator.LoadLinks(modelList, pageId, true, false, true).ConfigureAwait(false);

            return modelList.Select(x => x.NodeId).ToList();
        }
    }
}
