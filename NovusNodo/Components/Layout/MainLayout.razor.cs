using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using NovusNodo.Management;
using NovusNodoCore.NodeDefinition;

namespace NovusNodo.Components.Layout
{
    /// <summary>
    /// Represents the main layout component.
    /// </summary>
    public partial class MainLayout : LayoutComponentBase, IDisposable
    {
        private bool _drawerOpen = true;
        private MudTheme _theme = null;
        private DotNetObjectReference<NovusUIManagement> novusUIManagementRef;

        // To detect redundant calls
        private bool _disposedValue;
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

        private async Task ExecutionManager_DebugLogChanged((string, System.Text.Json.Nodes.JsonObject) arg)
        {
            await InvokeAsync(() =>
            {
                base.StateHasChanged();
            });
        }

        /// <summary>
        /// Toggles the state of the drawer.
        /// </summary>
        private void DrawerToggle()
        {
            _drawerOpen = !_drawerOpen;
        }

        /// <summary>
        /// Toggles between dark mode and light mode.
        /// </summary>
        private async Task DarkModeToggle()
        {
            NovusUIManagement.IsDarkMode = !NovusUIManagement.IsDarkMode;
            await NovusUIManagement.ReloadPage().ConfigureAwait(false);
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
                await JS.InvokeVoidAsync("GJSInitSettingsSideBar").ConfigureAwait(false);
                novusUIManagementRef = DotNetObjectReference.Create(NovusUIManagement);
                await JS.InvokeVoidAsync("GJSSetNovusReference", novusUIManagementRef).ConfigureAwait(false);
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
                }

                _disposedValue = true;
            }
        }

        private Palette GetCurrentPalette()
        {
            return NovusUIManagement.GetCurrentPalette();
        }
    }
}
