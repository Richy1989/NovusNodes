using System.Text;
using System.Text.Json.Nodes;
using NovusNodoPluginLibrary;
using NovusNodoPluginLibrary.Helper;

namespace NovusNodoPlugins
{
    /// <summary>
    /// Plugin class for debugging nodes.
    /// </summary>
    public class DebugNodePlugin : PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DebugNodePlugin"/> class.
        /// </summary>
        public DebugNodePlugin()
        {
            JsonConfig = "{\"DebugPath\": \"msg.time\"}";
        }

        /// <summary>
        /// Gets the unique identifier for the plugin.
        /// </summary>
        public override string ID { get; } = "B52F8BEC-BFAA-44D6-9AC2-D2565711A210";

        /// <summary>
        /// Gets the name of the plugin.
        /// </summary>
        public override string Name { get; } = "Debug";

        /// <summary>
        /// Gets the type of the node.
        /// </summary>
        public override NodeType NodeType { get; } = NodeType.Finisher;

        /// <summary>
        /// Defines the workload to be executed by the node.
        /// </summary>
        /// <returns>A function representing the asynchronous workload.</returns>
        public override Func<Task<string>> Workload(string jsonData)
        {
            return async () =>
            {
                var variables = await JsonVariableExtractor.ExtractVariablesAsync(jsonData).ConfigureAwait(false);

                foreach (var variable in variables)
                {
                    Console.WriteLine($"{variable.Key}: {variable.Value}");
                }

                return await Task.FromResult(string.Empty).ConfigureAwait(false);
            };
        }
    }
}
