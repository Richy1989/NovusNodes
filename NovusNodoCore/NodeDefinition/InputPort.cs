namespace NovusNodoCore.NodeDefinition
{
    /// <summary>
    /// Represents an input port in a node.
    /// </summary>
    public class InputPort(INodeBase node) : NodePort(node)
    {
        /// <summary>
        /// Gets or sets the dictionary of connected output ports.
        /// </summary>
        public Dictionary<string, OutputPort> ConnectedOutputPort { get; set; } = [];

        /// <summary>
        /// Removes all connections from this input port.
        /// </summary>
        public void RemoveAllConnections()
        {
            foreach (var outputPort in ConnectedOutputPort)
            {
                outputPort.Value.NextNodes.Remove(Id);
            }
            ConnectedOutputPort.Clear();
        }
    }
}
