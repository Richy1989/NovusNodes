using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.JSInterop;
using MudBlazor;
using NovusNodo.Components.Pages;
using NovusNodoCore.Managers;
using NovusNodoCore.NodeDefinition;

namespace NovusNodo.Management
{
    /// <summary>
    /// Manages the UIType settings for the Novus application.
    /// </summary>
    public class NovusUIManagement : IDisposable
    {
        public event Func<string, string, string, Task> OnNodeNameChanged;

        public event Func<Task> OnResetZoom;

        public event Func<bool, Task> OnNodeEnabledChanged;

        public NodeBase CurrentlySelectedNode { get; set; }
        public string CurrentlyOpenedPage { get; set; }

        private readonly ILogger<NovusUIManagement> Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NovusUIManagement"/> class with the specified execution manager.
        /// </summary>
        /// <param name="executionManager">The execution manager.</param>
        public NovusUIManagement(ILogger<NovusUIManagement> logger, ExecutionManager executionManager)
        {
            this.Logger = logger;
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
        public event Func<INodeBase, Task> OnNodeDoubleClicked;

        /// <summary>
        /// Event fired when the dark theme is changed.
        /// </summary>
        public event Func<bool, Task> OnDarkThemeChanged;

        /// <summary>
        /// Gets or sets the type of the sidebar UIType.
        /// </summary>
        public Type SideBarUI { get; set; } = typeof(BlankConfig);

        // To detect redundant calls
        private bool _disposedValue;

        /// <summary>
        /// Gets or sets a value indicating whether the dark mode is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if dark mode is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool isDarkMode { get; set; } = true;
        public bool IsDarkMode
        {
            get
            {
                return isDarkMode;
            }
            set
            {
                isDarkMode = value;
                if (OnDarkThemeChanged != null)
                {
                    OnDarkThemeChanged.Invoke(value).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the drawer settings are open.
        /// </summary>
        public bool SettingsDrawerOpen { get; set; } = true;
        public bool DrawerOpen { get; set; } = true;

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
        /// Gets the type of the settings UIType.
        /// </summary>
        /// <returns>The type of the settings UIType.</returns>
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
            await JS.InvokeVoidAsync("GJSReloadPage").ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the node double-click event from JavaScript.
        /// </summary>
        /// <param name="id">The ID of the node.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task NodeDoubleClicked(string pageid, string id)
        {
            CurrentlySelectedNode = ExecutionManager.NodePages[pageid].AvailableNodes[id];

            if (CurrentlySelectedNode != null && CurrentlySelectedNode.UIType != null)
            {
                SideBarUI = CurrentlySelectedNode.UIType;
            }

            await OnNodeDoubleClicked(CurrentlySelectedNode);
        }

        public async Task NodeEnabledChanged(bool isEnabled)
        {
            CurrentlySelectedNode.UIConfig.IsEnabled = isEnabled;   
           
            if (OnNodeEnabledChanged != null)
            {
                await OnNodeEnabledChanged.Invoke(isEnabled).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Changes the name of the currently selected node.
        /// </summary>
        public async Task ChangeNodeLabelName(string newName)
        {
            await OnNodeNameChanged?.Invoke(CurrentlyOpenedPage, CurrentlySelectedNode.Id, newName);
            CurrentlySelectedNode.Name = newName;

            //CurrentlySelectedNode.Name = newName;
            //try
            //{
            //    await CanvasRef.InvokeVoidAsync("setNodeName", [$"{CurrentlySelectedNode.Id}", $"{newName}"]);
            //}
            //catch (Exception ex)
            //{
            //    Logger.LogError(ex, "Failed to change node label.");
            //}
        }

        public async Task ResetZoom()
        {
            if (OnResetZoom != null)
            {
                await OnResetZoom.Invoke().ConfigureAwait(false);
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
                }

                _disposedValue = true;
            }
        }
    }
}
