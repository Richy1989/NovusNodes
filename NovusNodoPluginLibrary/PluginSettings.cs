namespace NovusNodoPluginLibrary
{
    /// <summary>
    /// Represents the settings for a plugin in the Novus Nodo Plugin Library.
    /// </summary>
    public class PluginSettings
    {
        /// <summary>
        /// Gets or sets the type of node.
        /// </summary>
        public NodeType NodeType { get; set; }

        /// <summary>
        /// Gets or sets the path to the start icon.
        /// </summary>
        public string StartIconPath { get; set; }

        /// <summary>
        /// Gets or sets the path to the end icon.
        /// </summary>
        public string EndIconPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the plugin is manually injectable.
        /// </summary>
        public bool IsManualInjectable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the plugin can be turned off.
        /// </summary>
        public bool IsSwitchable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the plugin is switched on.
        /// </summary>
        public bool IsSwitchedOn { get; set; } = true;

        public PluginSettings Clone()
        {
            return new PluginSettings
            {
                NodeType = NodeType,
                StartIconPath = StartIconPath,
                EndIconPath = EndIconPath,
                IsManualInjectable = IsManualInjectable,
                IsSwitchable = IsSwitchable,
                IsSwitchedOn = IsSwitchedOn
            };
        }
    }
}
