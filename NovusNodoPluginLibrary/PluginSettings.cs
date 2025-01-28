using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovusNodoPluginLibrary
{
    public class PluginSettings
    {
        public NodeType NodeType { get; set; }
        public string StartIconPath { get; set; }
        public string EndIconPath { get; set; }
        public bool IsManualInjectable { get; set; }
        public bool CanBeTurnedOff { get; set; }
    }
}
