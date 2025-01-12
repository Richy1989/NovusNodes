using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using NovusNodo.Management;

namespace NovusNodo.Components.Layout
{
    /// <summary>
    /// Represents the main layout component.
    /// </summary>
    public partial class MainLayout : LayoutComponentBase, IDisposable
    {
        private MudTheme _theme = null;
        private DotNetObjectReference<NovusUIManagement> novusUIManagementRef;
        private bool _disposedValue;
        private bool autoScrollDebugWindow = true;
        /// <summary>
        /// Initializes the component.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            ExecutionManager.DebugLogChanged += ExecutionManager_DebugLogChanged;

            _theme = new()
            {
                PaletteLight = NovusUIManagement.LightPalette,
                PaletteDark = NovusUIManagement.DarkPalette,
                LayoutProperties = new LayoutProperties()
            };
        }

        /// <summary>
        /// Handles the DebugLogChanged event from the ExecutionManager.
        /// </summary>
        /// <param name="arg">The argument containing the debug log information.</param>
        private async Task ExecutionManager_DebugLogChanged((string, System.Text.Json.Nodes.JsonObject) arg)
        {
            await InvokeAsync(async () =>
            {
                StateHasChanged();

                if (autoScrollDebugWindow)
                    await ScrollManager.ScrollToBottomAsync(".scrollable-drawer-content", ScrollBehavior.Smooth);
            });
        }

        /// <summary>
        /// Toggles the state of the drawer.
        /// </summary>
        private void DrawerToggle()
        {
            NovusUIManagement.DrawerOpen = !NovusUIManagement.DrawerOpen;
        }

        public async Task ClearDebugLog()
        {
            ExecutionManager.DebugLog.Clear();
            await InvokeAsync(() =>
            {
                StateHasChanged();
            });
        }

        /// <summary>
        /// Toggles the state of the settings drawer.
        /// </summary>
        private void SettingsDrawerToggle()
        {
            NovusUIManagement.SettingsDrawerOpen = !NovusUIManagement.SettingsDrawerOpen;
        }

        /// <summary>
        /// Toggles between dark mode and light mode.
        /// </summary>
        private async Task DarkModeToggle()
        {
            NovusUIManagement.IsDarkMode = !NovusUIManagement.IsDarkMode;
            await InvokeAsync(() =>
            {
                StateHasChanged();
            });
        }

        /// <summary>
        /// Gets the icon for the dark/light mode toggle button.
        /// </summary>
        public string DarkLightModeButtonIcon => NovusUIManagement.IsDarkMode switch
        {
            true => Icons.Material.Rounded.AutoMode,
            false => Icons.Material.Outlined.DarkMode,
        };

        /// <summary>
        /// Method called after the component has rendered.
        /// </summary>
        /// <param name="firstRender">Indicates if this is the first render.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                NovusUIManagement.JS = JS;

                novusUIManagementRef = DotNetObjectReference.Create(NovusUIManagement);
                await JS.InvokeVoidAsync("GJSSetNovusUIManagementRef", novusUIManagementRef);
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
                    novusUIManagementRef?.Dispose();
                    ExecutionManager.DebugLogChanged -= ExecutionManager_DebugLogChanged;
                }

                _disposedValue = true;
            }
        }
    }
}
