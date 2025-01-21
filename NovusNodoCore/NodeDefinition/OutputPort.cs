namespace NovusNodoCore.NodeDefinition
{
    /// <summary>
    /// Represents an output port in a node.
    /// </summary>
    public class OutputPort(INodeBase current) : NodePort(current)
    {
        /// <summary>
        /// Gets or sets the dictionary of next nodes connected to this output port.
        /// </summary>
        public Dictionary<string, INodeBase> NextNodes { get; set; } = [];

        /// <summary>
        /// Adds a connection from this output port to the specified input port of the next node.
        /// </summary>
        /// <param name="connectedPortID">The ID of the connected input port.</param>
        /// <param name="nextNode">The next node to connect to.</param>
        public void AddConnection(string connectedPortID, INodeBase nextNode)
        {
            NextNodes.Add(connectedPortID, nextNode);
            nextNode.InputPort.ConnectedOutputPort.Add(Id, this);
        }

        /// <summary>
        /// Removes the connection from this output port to the specified input port.
        /// </summary>
        /// <param name="connectedPortID">The ID of the connected input port to remove.</param>
        public void RemoveConnection(string connectedPortID)
        {
            NextNodes[connectedPortID].InputPort.ConnectedOutputPort.Remove(Id);
            NextNodes.Remove(connectedPortID);
        }

        public void RemoveAllConnections()
        {
            foreach (var nextNode in NextNodes)
            {
                nextNode.Value.InputPort.ConnectedOutputPort.Remove(Id);
            }
            NextNodes.Clear();
        }
    }
}
