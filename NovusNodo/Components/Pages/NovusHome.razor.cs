using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NovusNodoCore.NodeDefinition;
using NovusNodoPluginLibrary;

namespace NovusNodo.Components.Pages
{
    public partial class NovusHome : ComponentBase, IDisposable
    {
        // To detect redundant calls
        private bool _disposedValue;

        /// <summary>
        /// Delegate for the redraw connections action.
        /// </summary>
        private static Func<string, string, string, string, Task>? FunctionAddNewConnectionAsync;

        /// <summary>
        /// Redraws the connections asynchronously.
        /// </summary>
        /// <param name="sourceId">The source node identifier.</param>
        /// <param name="targetId">The target node identifier.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task LocalFunctionAddNewConnectionAsync(string sourceId, string sourcePortId, string targetId, string targetPortId)
        {
            ExecutionManager.NewConnection(sourceId, sourcePortId, targetId, targetPortId);
            await Task.CompletedTask;
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
            items = ExecutionManager.AvailableNodes;
            ExecutionManager.AvailableNodesUpdated += NodesAdded;
        }

        /// <summary>
        /// Invokable method to handle the movement of an element.
        /// </summary>
        /// <param name="id">The identifier of the element.</param>
        /// <param name="x">The new x-coordinate of the element.</param>
        /// <param name="y">The new y-coordinate of the element.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [JSInvokable]
        public static async Task ElementMoved(string id, double x, double y)
        {
            items[id].UIConfig.X = x;
            items[id].UIConfig.Y = y;
            await Task.CompletedTask;
        }

        /// <summary>
        /// Invokable method to handle the addition of a link.
        /// </summary>
        /// <param name="sourceID">The source node identifier.</param>
        /// <param name="targetId">The target node identifier.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [JSInvokable]
        public static async Task LinkAdded(string sourceID, string sourcePortId, string targetId, string targetPortId)
        {
            FunctionAddNewConnectionAsync?.Invoke(sourceID, sourcePortId, targetId, targetPortId);
            await Task.CompletedTask;
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
            await AddPorts(node).ConfigureAwait(false);
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

                    await AddPorts(node).ConfigureAwait(false);

                    await DrawLinks().ConfigureAwait(false);
                }
            }
        }

        private async Task DrawLinks()
        {
            foreach (var node in items.Values)
            {
                foreach (var ports in node.OutputPort.ConnectedInputPorts)
                {
                    await JS.InvokeVoidAsync("JJSCreateLink", [$"{node.ID}", $"{node.OutputPort.ID}", $"{ports}"]);
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
                await JS.InvokeVoidAsync("JJSAddInputPorts", [$"{node.ID}", $"{node.NodeType}"]);
            if (node.NodeType == NodeType.Worker || node.NodeType == NodeType.Starter)
                await JS.InvokeVoidAsync("JJSAddOutputPorts", [$"{node.ID}", $"{node.NodeType}"]);
        }

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
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