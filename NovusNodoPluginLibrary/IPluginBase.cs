namespace NovusNodoPluginLibrary
{
    public interface IPluginBase
    {
        /// <summary>
        /// Gets or sets the JSON configuration.
        /// </summary>
        string JsonConfig { get; set; }
        
        /*
        /// <summary>
        /// Gets the configuration object.
        /// </summary>
        /// <returns>The configuration object of type <typeparamref name="ConfigType"/>.</returns>
        ConfigType GetConfig();

        /// <summary>
        /// Sets the configuration object.
        /// </summary>
        /// <param name="config">The configuration object of type <typeparamref name="ConfigType"/>.</param>
        void SetConfig(ConfigType config);
        */

        /// <summary>
        /// Gets or sets the parent node.
        /// </summary>
        INodeBase ParentNode { get; set; }

        /// <summary>
        /// Gets the unique identifier for the plugin.
        /// </summary>
        string ID { get; }

        /// <summary>
        /// Gets the type of the node.
        /// </summary>
        NodeType NodeType { get; }

        /// <summary>
        /// Prepares the workload asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task PrepareWorkloadAsync();

        /// <summary>
        /// Defines the workload to be executed by the node.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Func<Task<string>> Workload(string jsonData);
    }
}
