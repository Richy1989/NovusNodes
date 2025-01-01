using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovusNodoCore.NodeDefinition
{
    public class OutputPort : NodePort
    {
        public List<string> ConnectedInputPorts { get; set; } = [];
        public Dictionary<string, INodeBase> NextNodes { get; set; } = [];
        
        public OutputPort(INodeBase current) : base(current)
        {

        }

        public void AddConnection(string connectedPortID, INodeBase nextNode)
        {
            NextNodes.Add(connectedPortID, nextNode);
        }

    }
}
