namespace NovusNodoCore.NodeDefinition
{
    /// <summary>
    /// Represents a port in a node, which can be either an input or output port.
    /// </summary>
    public class NodePort(bool isInputPort)
    {
        public List<string> ConnectedInputPorts { get; set; } = [];

        /// <summary>
        /// Gets or sets a value indicating whether this port is an input port.
        /// </summary>
        public bool IsInputPort { get; set; } = isInputPort;

        /// <summary>
        /// Gets or sets the unique identifier for this port.
        /// </summary>
        public string ID { get; set; }
    }
}
