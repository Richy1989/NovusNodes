using System.Reflection;
using System.Text.Json.Nodes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.Logging;
using NovusNodoPluginLibrary;

namespace NovusNodoUIPlugins.NetFunctionNode
{
    [NovusPlugin("B72CBE0A-F8D6-494D-B18E-FAE5A2369B60", ".NET Function", "#9966CC")]
    public class NetFunctionPlugin : PluginBase
    {
        public override NodeType NodeType => NodeType.Worker;
        private object instance;
        private Type type;

        public NetFunctionPlugin()
        {
            UIType = typeof(string);
            PluginConfig = @"
                            using System;
                            using System.Text.Json.Nodes;

                            namespace NetFunctionPlugin.CustomCodeClass
                            {
                                public class Writer
                                {
                                    public JsonObject RunCustomCode(JsonObject jsonData)
                                    {
                                        jsonData[""testVariable""] = ""Test from the inside!"";
                                        Console.WriteLine(""Hello From Inside""); return jsonData;
                                    }
                                }
                            }";

            AddWorkTask(Workload);
        }

        /// <summary>
        /// Defines the workload to be executed by the node.
        /// </summary>
        /// <param name="jsonData">The JSON data to be processed by the workload.</param>
        /// <returns>A task that represents the asynchronous operation and returns a string result.</returns>
        public async Task<JsonObject> Workload(JsonObject jsonData)
        {
            Logger.LogDebug($"Executing custom code: \n {PluginConfig}");
            PrepareCode();

            if (type != null && instance != null)
            {
                var userReturn = type.InvokeMember("RunCustomCode",
                            BindingFlags.Default | BindingFlags.InvokeMethod,
                            null,
                            instance,
                            [jsonData]);

                return await Task.FromResult((JsonObject)userReturn).ConfigureAwait(false);
            }

            return await Task.FromResult(jsonData).ConfigureAwait(false);
        }

        public void PrepareCode()
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText((string)PluginConfig);

            string assemblyName = Path.GetRandomFileName();
            MetadataReference[] references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(System.Object).Assembly.Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Console").Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
                MetadataReference.CreateFromFile(typeof(JsonObject).Assembly.Location)
            };

            // analyse and generate IL code from syntax tree
            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: [syntaxTree],
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                // write IL code into memory
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    // handle exceptions
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                    {
                        Logger.LogError("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                        //Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                    }
                }
                else
                {
                    // load this 'virtual' DLL so that we can use
                    ms.Seek(0, SeekOrigin.Begin);
                    Assembly assembly = Assembly.Load(ms.ToArray());

                    // create instance of the desired class and call the desired function
                    type = assembly.GetType("NetFunctionPlugin.CustomCodeClass.Writer");
                    instance = Activator.CreateInstance(type);
                }
            }
        }
    }
}
