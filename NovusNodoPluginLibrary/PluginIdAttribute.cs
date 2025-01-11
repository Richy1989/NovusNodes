namespace NovusNodoPluginLibrary
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class PluginIdAttribute : Attribute
    {
        /// <summary>
        /// Gets the unique identifier for the plugin.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets or sets the name of the plugin.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the background color of the plugin.
        /// </summary>
        public string Background { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginIdAttribute"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the plugin.</param>
        /// <param name="name">The name of the plugin.</param>
        /// <param name="hexColor">The background color of the plugin in hexadecimal format.</param>
        public PluginIdAttribute(string id, string name, string hexColor)
        {
            Name = name;
            Background = hexColor;
            Id = id;
        }
    }
}