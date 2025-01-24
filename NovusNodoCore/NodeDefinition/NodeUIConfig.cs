using System.Xml.Linq;

namespace NovusNodoCore.NodeDefinition
{
    /// <summary>
    /// Represents the configuration for the UIType of a node.
    /// </summary>
    public class NodeUIConfig
    {
        /// <summary>
        /// Gets or sets the X coordinate of the node.
        /// Default value is -1.
        /// </summary>
        public double X { get; set; } = 100;

        /// <summary>
        /// Gets or sets the Y coordinate of the node.
        /// Default value is -1.
        /// </summary>
        public double Y { get; set; } = 100;

        /// <summary>
        /// Gets or sets the name of the node.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the node is enabled.
        /// </summary>
        public bool IsEnabled { get; set; } = true;
    }
}