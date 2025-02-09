namespace NovusNodoUIPlugins.SplitCondition
{
    /// <summary>
    /// Represents a condition used for splitting operations.
    /// </summary>
    public class SplitCondition
    {
        /// <summary>
        /// Gets or sets the path of the variable involved in the condition.
        /// </summary>
        public string VariablePath { get; set; }

        /// <summary>
        /// Gets or sets the operator used in the condition.
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// Gets or sets the value to be compared in the condition.
        /// </summary>
        public string Value { get; set; }
    }

    /// <summary>
    /// Configuration class for managing multiple split conditions.
    /// </summary>
    public class SplitConditionConfig
    {
        /// <summary>
        /// Gets or sets the dictionary of split conditions.
        /// </summary>
        public Dictionary<string, SplitCondition> SplitConditions { get; set; } = new Dictionary<string, SplitCondition>();
    }
}
