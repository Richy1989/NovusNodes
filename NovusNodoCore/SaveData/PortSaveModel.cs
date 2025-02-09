namespace NovusNodoCore.SaveData
{
    /// <summary>
    /// Represents the model for saving port data.
    /// </summary>
    public class PortSaveModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the port.
        /// </summary>
        public string PortId { get; set; }

        /// <summary>
        /// Gets or sets the identifier for the related worker task.
        /// </summary>
        public string RelatedWorkerTaskId { get; set; }
    }
}
