using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovusNodoCore.NodeDefinition
{
    internal class NodeConnection
    {
        public INodeBase NextNode { get; set; }
        public NodePort NodePort { get; set; }
    }
}
