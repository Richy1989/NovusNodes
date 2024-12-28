namespace NovusNodoCore.NodeDefinition
{
    /// <summary>
    /// Represents the base class for execution results.
    /// </summary>
    public class ExecutionResultBase
    {
        /// <summary>
        /// Gets or sets the type of the execution result.
        /// </summary>
        public required Type Type { get; set; }

        /// <summary>
        /// Gets or sets the result of the execution.
        /// </summary>
        public required object Result { get; set; }
    }
}