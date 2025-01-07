using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NovusNodoCore.NodeDefinition;
using NovusNodoPluginLibrary;

namespace NovusNodoCore.SaveData
{
    internal class NodeSaveModel
    {
        public InputPort InputPort { get; set; }
        public Dictionary<string, OutputPort> OutputPorts { get; set; }
        public bool IsEnabled { get; set; }
        public string NodeID { get; set; }
        public string PageID { get; set; }
        public string PageName { get; set; }

        public Color BackgroundColor { get; set; }
        public NodeUIConfig NodeUIConfig { get; set; }
        public NodeType NodeType { get; set; }
        public Type PluginBaseType { get; set; }
    }
}
