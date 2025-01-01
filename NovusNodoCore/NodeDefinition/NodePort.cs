namespace NovusNodoCore.NodeDefinition
{
    /// <summary>
    /// Represents a port in a node, which can be either an input or output port.
    /// </summary>
    public abstract class NodePort
    {
        public NodePort(INodeBase node)
        {
            Node = node;
            ID = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Gets or sets the unique identifier for this port.
        /// </summary>
        public string ID { get; set; }

        public INodeBase Node { get; set; }
    }
}
