namespace NovusNodoCore.NodeDefinition
{
    /// <summary>
    /// Represents the configuration for the UI of a node.
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
        /// Gets or sets the width of the node.
        /// Default value is -1.
        /// </summary>
        public double Width { get; set; } = 120;

        /// <summary>
        /// Gets or sets the height of the node.
        /// Default value is -1.
        /// </summary>
        public double Height { get; set; } = 40;
    }
}