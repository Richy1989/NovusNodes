using System.Reflection;
using System.Text.Json.Nodes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.Logging;
using NovusNodoPluginLibrary;

namespace NovusNodoUIPlugins.NetFunctionNode
{
    /// <summary>
    /// Represents a plugin that executes .NET functions dynamically.
    /// </summary>
    [NovusPlugin("B72CBE0A-F8D6-494D-B18E-FAE5A2369B60", ".NET Function", "#9966CC")]
    public class NetFunctionPlugin : PluginBase
    {
        /// <summary>
        /// Gets the type of the node, which is a worker.
        /// </summary>
        public override NodeType NodeType => NodeType.Worker;

        private object instance;
        private Type type;
        private string oldSourceCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetFunctionPlugin"/> class.
        /// </summary>
        public NetFunctionPlugin()
        {
            UIType = typeof(NetFunctionPluginUI);
            StartIconPath = "Logo_C_sharp.png";
            AddWorkTask(Workload);
        }

        public override Task PrepareWorkloadAsync()
        {
            PrepareCode();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Defines the workload to be executed by the node.
        /// </summary>
        /// <param name="jsonData">The JSON data to be processed by the workload.</param>
        /// <returns>A task that represents the asynchronous operation and returns a JSON object result.</returns>
        public async Task<JsonObject> Workload(JsonObject jsonData)
        {
            Logger.LogDebug($"Executing custom code: \n {PluginConfig}");

            if (oldSourceCode != ((NetFunctionConfig)PluginConfig).SourceCode)
            {
                if (PrepareCode())
                    oldSourceCode = ((NetFunctionConfig)PluginConfig).SourceCode;
            }

            if (type != null && instance != null)
            {
                try
                {
                    var userReturn = type.InvokeMember("RunCustomCode",
                                BindingFlags.Default | BindingFlags.InvokeMethod,
                                null,
                                instance,
                                [jsonData]);

                    return await Task.FromResult((JsonObject)userReturn).ConfigureAwait(false);
                }
                catch (TargetInvocationException ex)
                {
                    // Unwrap the inner exception to get the actual exception thrown by the invoked method
                    Exception innerException = ex.InnerException;

                    if (innerException != null)
                    {
                        Logger.LogError($"Exception in invoked method: {innerException.Message}");
                        Logger.LogError($"Stack Trace: {innerException.StackTrace}");
                    }
                    else
                    {
                        Logger.LogError("TargetInvocationException occurred, but no inner exception is present.");
                    }
                }
                catch (Exception ex)
                {
                    // Catch any other unexpected exceptions
                    Logger.LogError($"Unexpected exception: {ex.Message}");
                    Logger.LogError($"Stack Trace: {ex.StackTrace}");
                }
            }

            return await Task.FromResult(jsonData).ConfigureAwait(false);
        }

        /// <summary>
        /// Prepares the code by compiling the source code in <see cref="PluginConfig"/>.
        /// </summary>
        /// <returns><c>true</c> if the code was successfully compiled and loaded; otherwise, <c>false</c>.</returns>
        public bool PrepareCode()
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(((NetFunctionConfig)PluginConfig).SourceCode);

            string assemblyName = Path.GetRandomFileName();
            var references = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
                .Select(a => MetadataReference.CreateFromFile(a.Location))
                .ToList();

            // Analyze and generate IL code from syntax tree
            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: [syntaxTree],
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                // Write IL code into memory
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    Logger.LogDebug("Error when Compile .NET custom code");
                    ((NetFunctionConfig)PluginConfig).LastCompileSuccess = false;

                    // Handle exceptions
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                    {
                        Logger.LogError("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                    }
                    return false;
                }
                else
                {
                    Logger.LogDebug("Compile .NET custom code Successfully");
                    ((NetFunctionConfig)PluginConfig).LastCompileSuccess = true;
                    // Load this 'virtual' DLL so that we can use
                    ms.Seek(0, SeekOrigin.Begin);
                    Assembly assembly = Assembly.Load(ms.ToArray());

                    // Create instance of the desired class and call the desired function
                    type = assembly.GetType("DotNetFunctionPlugin.DotNetCustomClass.DotNetCustomCode");
                    instance = Activator.CreateInstance(type);
                    return true;
                }
            }
        }
    }
}
