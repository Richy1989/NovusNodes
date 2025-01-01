using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovusNodoCore.NodeDefinition
{
    public class InputPort : NodePort
    {
        public string ConnectedOutputPort { get; set; } = "";

        public InputPort(INodeBase node) : base(node)
        {
        }
    }
}
