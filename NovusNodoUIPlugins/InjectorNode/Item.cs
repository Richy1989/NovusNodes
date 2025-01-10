using NovusNodoPluginLibrary.Enums;

namespace NovusNodoUIPlugins.InjectorNode
{
    /// <summary>
    /// Represents an item with an ID, variable, value, and selected type.
    /// </summary>
    public class Item
    {
        /// <summary>
        /// Gets the unique identifier for the item.
        /// </summary>
        public string ID { get; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the variable associated with the item.
        /// </summary>
        public string Variable { get; set; }

        /// <summary>
        /// Gets or sets the value associated with the item.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the selected type of the item.
        /// </summary>
        public PossibleTypesEnum SelectedType { get; set; }
    }
}
