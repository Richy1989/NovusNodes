﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;
using NovusNodo.Components.Pages;
using NovusNodo.Management;

namespace NovusNodo.Components.Layout
{
    /// <summary>
    /// Represents the main layout component.
    /// </summary>
    public partial class MainLayout : LayoutComponentBase
    {
        private bool _drawerOpen = true;
        private MudTheme _theme = null;

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
            _drawerOpen = !_drawerOpen;
        }

        /// <summary>
        /// Toggles between dark mode and light mode.
        /// </summary>
        private async Task DarkModeToggle()
        {
            NovusUIManagement.IsDarkMode = !NovusUIManagement.IsDarkMode;
            await JS.InvokeVoidAsync("reloadPage");

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
                await JS.InvokeVoidAsync("InitCustomSideBar");

            }
        }
    }
}
