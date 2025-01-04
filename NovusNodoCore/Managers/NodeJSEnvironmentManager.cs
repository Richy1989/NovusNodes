using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.JavaScript.NodeApi.Runtime;

namespace NovusNodoCore.Managers
{
    /// <summary>
    /// Using: https://microsoft.github.io/node-api-dotnet/scenarios/dotnet-js.html
    /// </summary>
    internal class NodeJSEnvironmentManager
    {
        public void Initialize()
        {
            // Initialize the NodeJS environment.
            var nodejs = new NodejsPlatform("C:\\Program Files\\nodejs\\node.exe").CreateEnvironment();
            Console.WriteLine(nodejs);
        }

    }
}
