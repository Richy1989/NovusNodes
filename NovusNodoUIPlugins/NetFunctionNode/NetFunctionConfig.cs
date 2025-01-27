namespace NovusNodoUIPlugins.NetFunctionNode
{
    /// <summary>
    /// Represents the configuration for a network function.
    /// </summary>
    class NetFunctionConfig
    {
        /// <summary>
        /// Gets or sets the source code of the network function.
        /// </summary>
        public string SourceCode { get; set; } = DefaultSourceCode;

        /// <summary>
        /// Gets or sets a value indicating whether the last compile was successful.
        /// </summary>
        public bool LastCompileSuccess { get; set; }

        private static readonly string DefaultSourceCode = 
@"using System;
using System.Text.Json.Nodes;

namespace DotNetFunctionPlugin.DotNetCustomClass
{
    public class DotNetCustomCode
    {
        public JsonObject RunCustomCode(JsonObject msg)
        {
            msg[""testVariable""] = ""Test from the inside!"";
            Console.WriteLine(""Hello From Inside"");
            return msg;
        }
    }
}";

    }
}
