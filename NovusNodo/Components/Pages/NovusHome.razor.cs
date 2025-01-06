using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NovusNodoCore.NodeDefinition;
using NovusNodoPluginLibrary;

namespace NovusNodo.Components.Pages
{
    /// <summary>
    /// Represents the home component for the Novus application.
    /// </summary>
    public partial class NovusHome : ComponentBase, IDisposable
    {
        // To detect redundant calls
        private bool _disposedValue;
        private DotNetObjectReference<NovusHome> novusHomeRef;
        /// <summary>
        /// Delegate for the redraw connections action.
        /// </summary>
        private static Func<string, string, string, string, Task> FunctionAddNewConnectionAsync;
        private static Func<string, string, string, string, Task> FunctionRemovedConnectionAsync;
        private static Func<string, Task> FunctionElementRemovedAsync;

        /// <summary>
        /// Redraws the connections asynchronously.
        /// </summary>
        /// <param name="sourceId">The source node identifier.</param>
        /// <param name="sourcePortId">The source port identifier.</param>
        /// <param name="targetId">The target node identifier.</param>
        /// <param name="targetPortId">The target port identifier.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task LocalFunctionAddNewConnectionAsync(string sourceId, string sourcePortId, string targetId, string targetPortId)
        {
            await InvokeAsync(() =>
            {
                ExecutionManager.NewConnection(sourceId, sourcePortId, targetId, targetPortId);
            });
        }

        /// <summary>
        /// Removes the connection asynchronously.
        /// </summary>
        /// <param name="sourceId">The source node identifier.</param>
        /// <param name="sourcePortId">The source port identifier.</param>
        /// <param name="targetId">The target node identifier.</param>
        /// <param name="targetPortId">The target port identifier.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task LocalFunctionRemovedConnectionAsync(string sourceId, string sourcePortId, string targetId, string targetPortId)
        {
            await InvokeAsync(() =>
            {
                ExecutionManager.RemoveConnection(sourceId, sourcePortId, targetId, targetPortId);
            });
        }

        /// <summary>
        /// Handles the removal of an element asynchronously.
        /// </summary>
        /// <param name="elementId">The identifier of the element to be removed.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task LocalFunctionElementRemovedAsync(string elementId)
        {
            await InvokeAsync(() =>
            {
                ExecutionManager.ElementRemoved(elementId);
            });
        }

        /// <summary>
        /// Dictionary to hold the available nodes.
        /// </summary>
        private static IDictionary<string, INodeBase> items = new Dictionary<string, INodeBase>();

        /// <summary>
        /// Method called when the component is initialized.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            FunctionAddNewConnectionAsync = LocalFunctionAddNewConnectionAsync;
            FunctionRemovedConnectionAsync = LocalFunctionRemovedConnectionAsync;
            FunctionElementRemovedAsync = LocalFunctionElementRemovedAsync;
            items = ExecutionManager.AvailableNodes;
            ExecutionManager.AvailableNodesUpdated += NodesAdded;
        }

        /// <summary>
        /// Sets the color palette for JointJS based on the current UI theme.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task SetJointJSColors()
        {
            if (NovusUIManagement.IsDarkMode)
            {
                await JS.InvokeVoidAsync("JJSSetColorPalette", [$"{NovusUIManagement.DarkPalette.Background}", "#ffffff", "#e8e8e8"]);
            }
            else
            {
                await JS.InvokeVoidAsync("JJSSetColorPalette", [$"{NovusUIManagement.LightPalette.Background}", "#1e1e2d", "#1e1e2d"]);
            }
        }

        /// <summary>
        /// Invokable method to handle the movement of an element.
        /// </summary>
        /// <param name="id">The identifier of the element.</param>
        /// <param name="x">The new x-coordinate of the element.</param>
        /// <param name="y">The new y-coordinate of the element.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [JSInvokable("NovusHome.ElementMoved")]
        public async Task ElementMoved(string id, double x, double y)
        {
            await InvokeAsync(() =>
            {
                items[id].UIConfig.X = x;
                items[id].UIConfig.Y = y;
            });
        }

        /// <summary>
        /// Invokable method to handle the resizing of an element.
        /// </summary>
        /// <param name="id">The identifier of the element.</param>
        /// <param name="width">The new width of the element.</param>
        /// <param name="height">The new height of the element.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [JSInvokable("NovusHome.ElementResized")]
        public async Task ElementResized(string id, double width, double height)
        {
            await InvokeAsync(() =>
            {
                items[id].UIConfig.Width = width;
                items[id].UIConfig.Height = height;
            });
        }

        /// <summary>
        /// Invokable method to handle the addition of a link.
        /// </summary>
        /// <param name="sourceID">The source node identifier.</param>
        /// <param name="sourcePortId">The source port identifier.</param>
        /// <param name="targetId">The target node identifier.</param>
        /// <param name="targetPortId">The target port identifier.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [JSInvokable("NovusHome.LinkAdded")]
        public async Task LinkAdded(string sourceID, string sourcePortId, string targetId, string targetPortId)
        {
            await InvokeAsync(() =>
            {
                ExecutionManager.NewConnection(sourceID, sourcePortId, targetId, targetPortId);
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
        [JSInvokable("NovusHome.LinkRemoved")]
        public async Task LinkRemoved(string sourceId, string sourcePortId, string targetId, string targetPortId)
        {
            await InvokeAsync(() =>
            {
                ExecutionManager.RemoveConnection(sourceId, sourcePortId, targetId, targetPortId);
            });
        }

        /// <summary>
        /// Invokable method to handle the deletion of an element.
        /// </summary>
        /// <param name="elementId">The identifier of the element to be deleted.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [JSInvokable("NovusHome.ElementRemoved")]
        public async Task ElementRemoved(string elementId)
        {
            await InvokeAsync(() =>
            {
                ExecutionManager.ElementRemoved(elementId);
            });
        }

        /// <summary>
        /// Event handler for when nodes are added.
        /// </summary>
        /// <param name="node">The node that was added.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task NodesAdded(INodeBase node)
        {
            items = ExecutionManager.AvailableNodes;
            await JS.InvokeVoidAsync("JJSCreateNodeElement", [$"{node.ID}", $"{ConvertColorToCSSColor(node.Background)}", $"{node.Name}"]);
            await AddPorts(node);
        }

        /// <summary>
        /// Method called after the component has rendered.
        /// </summary>
        /// <param name="firstRender">Indicates if this is the first render.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                novusHomeRef = DotNetObjectReference.Create(this);
                await JS.InvokeVoidAsync("GJSSetNovusHomeNetRef", novusHomeRef);

                await SetJointJSColors();
                await JS.InvokeVoidAsync("JJSCreatePaper", "main");

                foreach (var node in items.Values)
                {
                    if (node.UIConfig.X > 0 && node.UIConfig.Y > 0)
                    {
                        await JS.InvokeVoidAsync("JJSCreateNodeElement", [$"{node.ID}", $"{ConvertColorToCSSColor(node.Background)}", $"{node.Name}", $"{node.UIConfig.X}", $"{node.UIConfig.Y}"]);
                    }
                    else
                    {
                        await JS.InvokeVoidAsync("JJSCreateNodeElement", [$"{node.ID}", $"{ConvertColorToCSSColor(node.Background)}", $"{node.Name}"]);
                    }

                    await AddPorts(node);
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
            foreach (var node in items.Values)
            {
                foreach (var port in node.OutputPorts.Values)
                {
                    foreach (var nextNode in port.NextNodes)
                    {
                        string connectedPortId = nextNode.Key;
                        INodeBase connectedNode = nextNode.Value;

                        await JS.InvokeVoidAsync("JJSCreateLink", [$"{node.ID}", $"{port.ID}", $"{connectedNode.ID}", $"{connectedPortId}"]);
                    }
                }
            }
        }

        /// <summary>
        /// Converts a System.Drawing.Color to a CSS color string.
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <returns>The CSS color string.</returns>
        public static string ConvertColorToCSSColor(System.Drawing.Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        /// <summary>
        /// Adds input and output ports to the node.
        /// </summary>
        /// <param name="node">The node to add ports to.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task AddPorts(INodeBase node)
        {
            if (node.NodeType == NodeType.Worker || node.NodeType == NodeType.Finisher)
            {
                await JS.InvokeVoidAsync("JJSAddInputPort", [$"{node.ID}", $"{node.InputPort.ID}"]);
            }
            if (node.NodeType == NodeType.Worker || node.NodeType == NodeType.Starter)
            {
                foreach (var port in node.OutputPorts.Values)
                {
                    await JS.InvokeVoidAsync("JJSAddOutputPort", [$"{node.ID}", $"{port.ID}"]);
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
                    FunctionAddNewConnectionAsync = null;
                }

                _disposedValue = true;
            }
        }
    }
}