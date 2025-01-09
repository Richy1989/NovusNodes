using System.Runtime.Loader;
using System.Reflection;
using MudBlazor.Services;
using NovusNodo.Components;
using NovusNodo.Management;
using NovusNodoCore.Managers;
using NovusNodoCore;
using Microsoft.Extensions.Logging;
using NovusNodoPluginLibrary;
using Microsoft.AspNetCore.Components;
using NovusNodo.PluginManagement;

namespace NovusNodo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


          
        

        // Add logging to console
        builder.Logging.AddConsole();

            builder.Services.AddSingleton<NovusUIManagement>();

            builder.Services.AddNovusCoreComponents();

            // Add MudBlazor services
            builder.Services.AddMudServices();

            //Load Novus Plugins
            builder.Services.AddPluginComponents();
            builder.Services.LoadStaticFilesOfPlugins();
            builder.Services.AddSimplePluginComponents();

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            var app = builder.Build();

            //Initialize Novus Core
            ExecutionManager executionManager = app.Services.GetRequiredService<ExecutionManager>();
            executionManager.Initialize();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.MapStaticAssets();
            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
