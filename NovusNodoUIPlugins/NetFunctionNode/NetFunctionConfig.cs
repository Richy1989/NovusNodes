namespace NovusNodoUIPlugins.NetFunctionNode
{
    /// <summary>
    /// Represents the configuration for the .NET function plugin.
    /// </summary>
    class NetFunctionConfig
    {
        /// <summary>
        /// Gets or sets the source code of the function.
        /// </summary>
        /// <value>The source code of the network function.</value>
        public string SourceCode { get; set; } = DefaultSourceCode;

        /// <summary>
        /// Gets or sets a value indicating whether the last compile was successful.
        /// </summary>
        /// <value><c>true</c> if the last compile was successful; otherwise, <c>false</c>.</value>
        public bool LastCompileSuccess { get; set; }

        /// <summary>
        /// The default source code for the function.
        /// </summary>
        private static readonly string DefaultSourceCode =
@"using System;
    using System.Text.Json.Nodes;

    namespace DotNetFunctionPlugin.DotNetCustomClass
    {
        public class DotNetCustomCode
        {
            /// <summary>
            /// Runs the custom code with the provided JSON object.
            /// </summary>
            /// <param name=""msg"">The JSON object to process.</param>
            /// <returns>The processed JSON object.</returns>
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
