using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NovusNodo.Management;
using NovusNodoCore.Managers;
using NovusNodoCore.NodeDefinition;
using NovusNodoPluginLibrary;

namespace NovusNodo.Components.Pages
{
    /// <summary>
    /// Represents the JointJS Paper component for managing and rendering nodes and their connections.
    /// </summary>
    public partial class JointJSPaper : ComponentBase, IDisposable
    {
        /// <summary>
        /// Indicates whether the component has been initialized.
        /// </summary>
        private bool isInitialized = false;

        /// <summary>
        /// Indicates whether the object has been disposed.
        /// </summary>
        private bool _disposedValue;

        /// <summary>
        /// A reference to the current instance of the JointJS Paper component for JavaScript interop.
        /// </summary>
        private DotNetObjectReference<JointJSPaper> canvasNetComponentRef;

        /// <summary>
        /// Gets or sets the Tab ID associated with this JointJS Paper component.
        /// </summary>
        [Parameter]
        public string TabID { get; set; }

        [Parameter]
        public IJSObjectReference CanvasReference { get; set; }

        /// <summary>
        /// Gets or sets the NodePageManager instance for managing nodes on this page.
        /// </summary>
        public NodePageManager NodePageManager { get; set; }

        //private IJSObjectReference canvasRef;
        /// <summary>
        /// Method called when the component is initialized.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            ExecutionManager.OnCurveStyleChanged += ExecutionManager_OnCurveStyleChanged;
            NodePageManager = ExecutionManager.NodePages[TabID];
            NodePageManager.AvailableNodesUpdated += NodesAdded;
            NovusUIManagement.OnDarkThemeChanged += NovusUIManagement_OnDarkThemeChanged;
            NovusUIManagement.OnNodeNameChanged += NovusUIManagement_OnNodeNameChanged;
        }

        private async Task NovusUIManagement_OnNodeNameChanged(string tabId, string nodeId, string name)
        {
            if (TabID == tabId)
            {
                try
                {
                    await CanvasReference.InvokeVoidAsync("setNodeName", [$"{nodeId}", $"{name}"]);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to change node label.");
                }
            }
        }

        private async Task NovusUIManagement_OnDarkThemeChanged(bool arg)
        {
            await SetColorTheme(arg);
        }

        private async Task ExecutionManager_OnCurveStyleChanged(bool arg)
        {
            await CanvasReference.InvokeVoidAsync("setLineStyle", arg);
        }

        /// <summary>
        /// Sets the color palette for JointJS based on the current UI theme.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task SetColorTheme(bool isDarkTheme)
        {
            await CanvasReference.InvokeVoidAsync("setDarkMode", isDarkTheme);
        }

        /// <summary>
        /// Invokable method to handle the movement of an element.
        /// </summary>
        /// <param name="id">The identifier of the element.</param>
        /// <param name="x">The new x-coordinate of the element.</param>
        /// <param name="y">The new y-coordinate of the element.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [JSInvokable("NovusNode.ElementMoved")]
        public async Task ElementMoved(string id, double x, double y)
        {
            await InvokeAsync(() =>
            {
                NodePageManager.AvailableNodes[id].UIConfig.X = x;
                NodePageManager.AvailableNodes[id].UIConfig.Y = y;
            });
        }

        /// <summary>
        /// Invokable method to handle the click event of an injector element.
        /// </summary>
        /// <param name="id">The identifier of the injector element.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [JSInvokable("NovusNode.InjectorElementClicked")]
        public async Task InjectorElementClicked(string id)
        {
            Logger.LogDebug($"Injector Element Clicked {id} in Tab: {TabID}");
            await NodePageManager.AvailableNodes[id].TriggerManualExecute();
        }

        /// <summary>
        /// Invokable method to handle the resizing of an element.
        /// </summary>
        /// <param name="id">The identifier of the element.</param>
        /// <param name="width">The new width of the element.</param>
        /// <param name="height">The new height of the element.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [JSInvokable("NovusNode.NodeResized")]
        public async Task ElementResized(string id, double width, double height)
        {
            Logger.LogDebug($"Element Resized {id} {width} {height} in {TabID}");
            NodePageManager.AvailableNodes[id].UIConfig.Width = width;
            NodePageManager.AvailableNodes[id].UIConfig.Height = height;
            await Task.CompletedTask;
        }

        /// <summary>
        /// Invokable method to handle the addition of a link.
        /// </summary>
        /// <param name="sourceID">The source node identifier.</param>
        /// <param name="sourcePortId">The source port identifier.</param>
        /// <param name="targetId">The target node identifier.</param>
        /// <param name="targetPortId">The target port identifier.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [JSInvokable("NovusNode.LinkAdded")]
        public async Task LinkAdded(string sourceID, string sourcePortId, string targetId, string targetPortId)
        {
            Logger.LogDebug($"Link Added {sourceID} {sourcePortId} {targetId} {targetPortId} to {TabID}");
            await InvokeAsync(() =>
            {
                NodePageManager.NewConnection(sourceID, sourcePortId, targetId, targetPortId);
            });
        }

        /// <summary>
        /// Invokable method to handle the removal of a link.
        /// </summary>
        /// <param name="sourceId">The source node identifier.</param>
        /// <param name="sourcePortId">The source port identifier.</param>
        /// <param name="targetId">The target node identifier.</param>
        /// <param name="targetPortId">The target port identifier.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [JSInvokable("NovusNode.LinkRemoved")]
        public async Task LinkRemoved(string sourceId, string sourcePortId, string targetId, string targetPortId)
        {
            Logger.LogDebug($"Link Removed {sourceId} {sourcePortId} {targetId} {targetPortId} from {TabID}");
            await InvokeAsync(() =>
            {
                NodePageManager.RemoveConnection(sourceId, sourcePortId, targetId, targetPortId);
            });
        }

        /// <summary>
        /// Invokable method to handle the deletion of an element.
        /// </summary>
        /// <param name="elementId">The identifier of the element to be deleted.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [JSInvokable("NovusNode.ElementRemoved")]
        public async Task ElementRemoved(string elementId)
        {
            Logger.LogDebug($"Element Removed {elementId} from {TabID}");
            await InvokeAsync(() =>
            {
                NodePageManager.ElementRemoved(elementId);
            });
        }

        [JSInvokable("NovusNode.NodeDoubleClicked")]
        public async Task NodeDoubleClick(string pageId, string nodeId)
        {
            await NovusUIManagement.NodeDoubleClicked(pageId, nodeId);
        }

        /// <summary>
        /// Event handler for when nodes are added.
        /// </summary>
        /// <param name="node">The node that was added.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task NodesAdded(NodeBase node)
        {
            node.PropertyChanged += async (sender, e) =>
            {
                if (e.PropertyName == "IsEnabled")
                {
                    if (sender is NodeBase node)
                    {
                        await CanvasReference.InvokeVoidAsync("enableDisableNode", [node.Id, node.IsEnabled]);
                    }
                }
            };

            Logger.LogDebug($"Adding node {node.Id} to canvas {TabID}");
            await CanvasReference.InvokeVoidAsync("createNode", [node.Id, node.PluginIdAttribute.Background, node.Name, node.UIConfig.Width, node.UIConfig.Height, node.UIConfig.X, node.UIConfig.Y, (double)node.NodeType]);
            await AddPorts(node);
        }

        /// <summary>
        /// Method called after the component has rendered.
        /// </summary>
        /// <param name="firstRender">Indicates if this is the first render.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            base.OnAfterRender(firstRender);
            if (firstRender)
            {
                canvasNetComponentRef = DotNetObjectReference.Create(this);
                //CanvasReference = await JS.InvokeAsync<IJSObjectReference>("import", "./node_framework/node_framework.js");
                Logger.LogDebug("Creating canvas for tab {TabID}", TabID);
                await CanvasReference.InvokeVoidAsync("createCanvas", [TabID, canvasNetComponentRef]);
                await SetColorTheme(NovusUIManagement.IsDarkMode);

                if (!isInitialized)
                {
                    isInitialized = true;
                    foreach (var node in NodePageManager.AvailableNodes.Values)
                    {
                        await NodesAdded(node);
                    }
                }
                await DrawLinks();
            }
        }

        /// <summary>
        /// Draws the links between nodes.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task DrawLinks()
        {
            foreach (var node in NodePageManager.AvailableNodes.Values)
            {
                foreach (var port in node.OutputPorts.Values)
                {
                    foreach (var nextNode in port.NextNodes)
                    {
                        string connectedPortId = nextNode.Key;
                        INodeBase connectedNode = nextNode.Value;

                        await CanvasReference.InvokeVoidAsync("addLink", [node.Id, port.Id, connectedNode.Id, connectedPortId]);
                    }
                }
            }
        }

        /// <summary>
        /// Adds input and output ports to the node.
        /// </summary>
        /// <param name="node">The node to add ports to.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task AddPorts(NodeBase node)
        {
            if (node.NodeType == NodeType.Worker || node.NodeType == NodeType.Finisher)
            {
                await CanvasReference.InvokeVoidAsync("addInputPorts", [node.Id, node.InputPort.Id]);
            }
            if (node.NodeType == NodeType.Worker || node.NodeType == NodeType.Starter)
            {
                foreach (var port in node.OutputPorts.Values)
                {
                    await CanvasReference.InvokeVoidAsync("addOutputPorts", [node.Id, port.Id]);
                }
            }
        }

        /// <summary>
        /// Public implementation of Dispose pattern callable by consumers.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern.
        /// </summary>
        /// <param name="disposing">Indicates whether the method is called from Dispose.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Logger.LogDebug($"Disposing JointJS Paper ID: {TabID}");
                    ExecutionManager.OnCurveStyleChanged -= ExecutionManager_OnCurveStyleChanged;
                    NodePageManager.AvailableNodesUpdated -= NodesAdded;
                    NovusUIManagement.OnDarkThemeChanged -= NovusUIManagement_OnDarkThemeChanged;
                    NovusUIManagement.OnNodeNameChanged -= NovusUIManagement_OnNodeNameChanged;
                    canvasNetComponentRef?.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}
