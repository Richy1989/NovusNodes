using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.JavaScript.NodeApi;
using Microsoft.JavaScript.NodeApi.Runtime;

namespace NovusNodoCore.Managers
{
    /// <summary>
    /// Using: https://microsoft.github.io/node-api-dotnet/scenarios/dotnet-js.html
    /// </summary>
    public class NodeJSEnvironmentManager
    {
        // To detect redundant calls
        private bool _disposedValue;

        private readonly NodejsPlatform nodejsPlatform;
        private NodejsEnvironment nodejs;
        private string globalNovusJavaScriptPath = null;

        public void Initialize()
        {
            string executingDir = Directory.GetCurrentDirectory();
            string wwwrootDir = Path.Combine(executingDir, "wwwroot");
            globalNovusJavaScriptPath = Path.Combine(wwwrootDir, "JSFolder", "GlobalJS.js");
            string baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            
            string libnodePath = Path.Combine(executingDir, "../", "libnode", "out", "Release", "libnode.dll");
            // Initialize the NodeJS environment.
            NodejsPlatform nodejsPlatform = new(libnodePath);
            nodejs = nodejsPlatform.CreateEnvironment(baseDir);
        }


        public JsonObject RunUserCode(string code, JsonObject parameters)
        {
            string content = "";
            nodejs.Run(() =>
            {
                var globalNovusJavaScript = nodejs.Import(globalNovusJavaScriptPath);
                content = (string)globalNovusJavaScript.CallMethod("GJSRunUserCode", $"{code}", GetStringRepresentation(parameters));
            });

            var json = JsonSerializer.Deserialize<JsonObject>(content);

            if(json == null)
            {
                return [];
            }

            return json;
        }

        public string GetStringRepresentation(JsonObject jsonObject)
        {
            // Serialize the JsonObject to a JSON string
            return JsonSerializer.Serialize(jsonObject, new JsonSerializerOptions
            {
                WriteIndented = true // Optional: Makes the output more readable
            });
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
                    nodejs.Dispose();
                    nodejsPlatform.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}
