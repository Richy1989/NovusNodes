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
        private object instance;
        private Type type;
        private string oldSourceCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetFunctionPlugin"/> class.
        /// </summary>
        public NetFunctionPlugin()
        {
            UIType = typeof(NetFunctionPluginUI);

            PluginSettings = new PluginSettings
            {
                StartIconPath = "Logo_C_sharp.png",
                NodeType = NodeType.Worker,
            };

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
            PluginConfig = PluginConfig == null ? new NetFunctionConfig() : (NetFunctionConfig)PluginConfig;

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
                        Logger.LogError(innerException, $"Exception in invoked method");
                    }
                    else
                    {
                        Logger.LogError("TargetInvocationException occurred, but no inner exception is present.");
                    }
                }
                catch (Exception ex)
                {
                    // Catch any other unexpected exceptions
                    Logger.LogError(ex, $"Unexpected exception");
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

            //var references = AppDomain.CurrentDomain.GetAssemblies()
            //    .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
            //    .Select(a => MetadataReference.CreateFromFile(a.Location))
            //    .ToList();

            

            string assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

            //IEnumerable<PortableExecutableReference> defaultReferences = new[]
            //{
            //    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")),
            //    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")),
            //    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")),
            //    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),
            //    MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Linq.dll")),
            //    MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location),
            //    MetadataReference.CreateFromFile(typeof(JsonObject).Assembly.Location),
            //    MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
            //};

            var references = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
                .Select(a => MetadataReference.CreateFromFile(a.Location))
                .ToList();

            //references.AddRange(defaultReferences);

            CSharpCompilationOptions defaultCompilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithOverflowChecks(true)
            .WithOptimizationLevel(OptimizationLevel.Release);

            // Analyze and generate IL code from syntax tree
            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: [syntaxTree],
                references: references,
                options: defaultCompilationOptions);

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
                    Logger.LogDebug("Compiled .NET custom code Successfully");
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

        public override async Task StopPluginAsync()
        {
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
