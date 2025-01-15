using NovusNodoPluginLibrary.Enums;

namespace NovusNodoUIPlugins.InjectorNode
{
    /// <summary>
    /// Configuration class for the Injector Node.
    /// </summary>
    public class InjectorNodeConfig
    {
        /// <summary>
        /// Gets or sets the list of injector entries.
        /// </summary>
        public List<Item> InjectorEntries { get; set; } = [];

        /// <summary>
        /// Gets or sets the injection mode.
        /// </summary>
        public InjectMode InjectMode { get; set; } = InjectMode.None;

        /// <summary>
        /// Gets or sets the injection interval.
        /// </summary>
        public InjectInterval InjectInterval { get; set; } = InjectInterval.Seconds;

        /// <summary>
        /// Gets or sets the value of the injection interval.
        /// </summary>
        public double InjectIntervalValue { get; set; } = 5;

        /// <summary>
        /// Creates a default configuration for the Injector Node.
        /// </summary>
        /// <returns>A default <see cref="InjectorNodeConfig"/> instance.</returns>
        public static InjectorNodeConfig CreateDefault()
        {
            InjectorNodeConfig config = new()
            {
                InjectorEntries =
                [
                    new Item { Variable = "payload", Value = DateTime.UtcNow.ToString("O"), SelectedType = PossibleTypesEnum.DateTime }
                ]
            };
            return config;
        }
    }
}
