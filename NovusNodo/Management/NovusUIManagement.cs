using Microsoft.JSInterop;
using MudBlazor;
using NovusNodo.Components.Pages;
using NovusNodoCore.Managers;
using NovusNodoCore.NodeDefinition;

namespace NovusNodo.Management
{
    /// <summary>
    /// Manages the UI settings for the Novus application.
    /// </summary>
    public class NovusUIManagement : IDisposable
    {
        public INodeBase CurrentlySelectedNode { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="NovusUIManagement"/> class with the specified execution manager.
        /// </summary>
        /// <param name="executionManager">The execution manager.</param>
        public NovusUIManagement(ExecutionManager executionManager)
        {
            this.ExecutionManager = executionManager;
        }

        /// <summary>
        /// Gets or sets the JavaScript runtime.
        /// </summary>
        public IJSRuntime JS { get; set; }

        /// <summary>
        /// Gets the execution manager.
        /// </summary>
        private ExecutionManager ExecutionManager { get; set; }

        /// <summary>
        /// Occurs when a node is double-clicked.
        /// </summary>
        public event Func<INodeBase, Task> NodeDoubleClicked;

        /// <summary>
        /// Gets or sets the type of the sidebar UI.
        /// </summary>
        public Type SideBarUI { get; set; } = typeof(BlankConfig);

        // To detect redundant calls
        private bool _disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="NovusUIManagement"/> class.
        /// </summary>
        public NovusUIManagement()
        {

        }

        /// <summary>
        /// Gets or sets a value indicating whether the dark mode is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if dark mode is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsDarkMode { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the drawer settings are open.
        /// </summary>
        public bool DrawerSettingsOpen { get; set; } = true;

        /// <summary>
        /// Gets the light palette.
        /// </summary>
        public readonly PaletteLight LightPalette = new()
        {
            Black = "#110e2d",
            AppbarText = "#424242",
            AppbarBackground = "rgba(255,255,255,0.8)",
            DrawerBackground = "#ffffff",
            GrayLight = "#e8e8e8",
            GrayLighter = "#f9f9f9",
        };

        /// <summary>
        /// Gets the dark palette.
        /// </summary>
        public readonly PaletteDark DarkPalette = new()
        {
            Primary = "#7e6fff",
            Surface = "#1e1e2d",
            Background = "#1a1a27",
            BackgroundGray = "#151521",
            AppbarText = "#92929f",
            AppbarBackground = "rgba(26,26,39,0.8)",
            DrawerBackground = "#1a1a27",
            ActionDefault = "#74718e",
            ActionDisabled = "#9999994d",
            ActionDisabledBackground = "#605f6d4d",
            TextPrimary = "#b2b0bf",
            TextSecondary = "#92929f",
            TextDisabled = "#ffffff33",
            DrawerIcon = "#92929f",
            DrawerText = "#92929f",
            GrayLight = "#2a2833",
            GrayLighter = "#1e1e2d",
            Info = "#4a86ff",
            Success = "#3dcb6c",
            Warning = "#ffb545",
            Error = "#ff3f5f",
            LinesDefault = "#33323e",
            TableLines = "#33323e",
            Divider = "#292838",
            OverlayLight = "#1e1e2d80",
        };

        public Palette GetCurrentPalette()
        {
            if (IsDarkMode)
            {
                return DarkPalette;
            }
            return LightPalette;
        }

        /// <summary>
        /// Gets the type of the settings UI.
        /// </summary>
        /// <returns>The type of the settings UI.</returns>
        public Type GetSettingsUIType()
        {
            return null;
        }

        /// <summary>
        /// Reloads the page.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task ReloadPage()
        {
            //await Task.CompletedTask.ConfigureAwait(false);
            await JS.InvokeVoidAsync("GJSReloadPage").ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the node double-click event from JavaScript.
        /// </summary>
        /// <param name="id">The ID of the node.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        [JSInvokable("NovusUIManagement.CellDoubleClick")]
        public async Task JJSNodeDoubleClicked(string id)
        {
            CurrentlySelectedNode = ExecutionManager.AvailableNodes[id];

            if (CurrentlySelectedNode != null && CurrentlySelectedNode.UI != null)
            {
                SideBarUI = CurrentlySelectedNode.UI;
            }

            await NodeDoubleClicked(CurrentlySelectedNode);
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
                }

                _disposedValue = true;
            }
        }
    }
}
