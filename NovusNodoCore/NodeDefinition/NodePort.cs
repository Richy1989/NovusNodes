using System;

namespace NovusNodoCore.NodeDefinition
{
    /// <summary>
    /// Represents a port in a node, which can be either an input or output port.
    /// </summary>
    public abstract class NodePort(INodeBase node)
    {
        /// <summary>
        /// Gets or sets the unique identifier for this port.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public INodeBase Node { get; set; } = node;
    }
}
