namespace NovusNodoPluginLibrary
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class NovusPluginAttribute : Attribute
    {
        public string AssemblyName { get; set; }
        /// <summary>
        /// The plugin configuration type./ Will be set when the plugin is loaded
        /// </summary> 
        public Type PluginConfigType { get; set; }
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
        /// Initializes a new instance of the <see cref="NovusPluginAttribute"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the plugin.</param>
        /// <param name="name">The name of the plugin.</param>
        /// <param name="hexColor">The background color of the plugin in hexadecimal format.</param>
        public NovusPluginAttribute(string id, string name, string hexColor)
        {
            Name = name;
            Background = hexColor;
            Id = id;
        }
    }
}