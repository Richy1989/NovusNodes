using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.JavaScript.NodeApi.Generator;
using Microsoft.JavaScript.NodeApi.Runtime;

namespace NovusNodoCore.Managers
{
    /// <summary>
    /// Manages the NodeJS environment for executing JavaScript code within a .NET application.
    /// </summary>
    public class NodeJSEnvironmentManager : IDisposable
    {
        private readonly ILogger<NodeJSEnvironmentManager> logger;
        private bool _disposedValue;
        private NodejsPlatform nodejsPlatform;
        private NodejsEnvironment nodejs;
        private string globalNovusJavaScriptPath = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeJSEnvironmentManager"/> class.
        /// </summary>
        /// <param name="Logger">The logger instance for logging errors and information.</param>
        public NodeJSEnvironmentManager(ILogger<NodeJSEnvironmentManager> Logger)
        {
            logger = Logger;
        }

        /// <summary>
        /// Initializes the NodeJS environment and sets up the necessary paths.
        /// </summary>
        public void Initialize()
        {
            logger.LogDebug("Initializing NodeJS environment");

            string executingDir = Directory.GetCurrentDirectory();
            string wwwrootDir = Path.Combine(executingDir, "wwwroot");
            globalNovusJavaScriptPath = Path.Combine(wwwrootDir, "JSFolder", "BackendGlobal.js");
            string baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string libnodePath = GetLibNodePath();

            logger.LogDebug("Libnode path: {0}", libnodePath);
            logger.LogDebug("BackendGlobal path: {0}", globalNovusJavaScriptPath);

            // Initialize the NodeJS environment.
            nodejsPlatform = new(libnodePath);
            nodejs = nodejsPlatform.CreateEnvironment(baseDir);
            logger.LogInformation("NodeJS environment initialized");
        }

        /// <summary>
        /// Runs the specified user code within the NodeJS environment.
        /// </summary>
        /// <param name="code">The JavaScript code to execute.</param>
        /// <param name="parameters">The parameters to pass to the JavaScript code.</param>
        /// <returns>A <see cref="JsonObject"/> containing the result of the executed code.</returns>
        public JsonObject RunUserCode(string code, JsonObject parameters)
        {
            JsonObject content = [];
            nodejs.Run(() =>
            {
                try
                {
                    var globalNovusJavaScript = nodejs.Import(globalNovusJavaScriptPath);
                    var jsonNode = JsonObject.Parse((string)globalNovusJavaScript.CallMethod("RunUserCode", $"{code}", GetStringRepresentation(parameters)));
                    content = jsonNode.AsObject();
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error running user code.");
                }
            });

            //var userCodeRunner = (IUserJSCodeRunner)nodejs.Import<IUserJSCodeRunner>(globalNovusJavaScriptPath, "");

            return content;
        }

        /// <summary>
        /// Converts a <see cref="JsonObject"/> to its string representation.
        /// </summary>
        /// <param name="jsonObject">The <see cref="JsonObject"/> to convert.</param>
        /// <returns>A JSON string representation of the <see cref="JsonObject"/>.</returns>
        public string GetStringRepresentation(JsonObject jsonObject)
        {
            // Serialize the JsonObject to a JSON string
            return JsonSerializer.Serialize(jsonObject);
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

        /// <summary>
        /// Gets the path to the libnode.dll file.
        /// </summary>
        /// <returns>The path to the libnode.dll file.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the libnode.dll file is not found.</exception>
        private string GetLibNodePath()
        {
            string appDir = Path.GetDirectoryName(typeof(Program).Assembly.Location)!;
            string libnodePath = Path.Combine(appDir, "libnode.dll");

            if (File.Exists(libnodePath)) return libnodePath;

            string executingDir = Directory.GetCurrentDirectory();
            libnodePath = Path.Combine(executingDir, "../", "libnode", "out", "Release", "libnode.dll");

            if (File.Exists(libnodePath)) return libnodePath;

            logger.LogError("libnode.dll not found");
            throw new FileNotFoundException("libnode.dll not found");
        }
    }
}
