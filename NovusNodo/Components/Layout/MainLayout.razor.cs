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
        /// <summary>
        /// Initializes the component.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            _theme = new()
            {
                PaletteLight = NovusUIManagement.LightPalette,
                PaletteDark = NovusUIManagement.DarkPalette,
                LayoutProperties = new LayoutProperties()
            };
        }

        /// <summary>
        /// Toggles the state of the drawer.
        /// </summary>
        private void DrawerToggle()
        {
            NovusUIManagement.DrawerOpen = !NovusUIManagement.DrawerOpen;
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
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                NovusUIManagement.JS = JS;

                //var canvasRef = await JS.InvokeAsync<IJSObjectReference>("import", "./node_framework/node_framework.js");
                //NovusUIManagement.CanvasRef = canvasRef;

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
                }

                _disposedValue = true;
            }
        }
    }
}
