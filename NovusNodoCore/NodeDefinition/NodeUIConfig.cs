using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NovusNodoCore.NodeDefinition
{
    /// <summary>
    /// Represents the configuration for the UIType of a node.
    /// </summary>
    public class NodeUIConfig : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the X coordinate of the node.
        /// Default value is -1.
        /// </summary>
        private double x = 100;
        public double X
        {
            get => x;
            set
            {
                if (x != value)
                {
                    x = value; OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the Y coordinate of the node.
        /// Default value is -1.
        /// </summary>
        private double y = 100;
        public double Y
        {
            get => y;
            set
            {
                if (y != value)
                {
                    y = value; OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the node.
        /// </summary>
        private string name;
        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value; OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the node is enabled.
        /// </summary>
        private bool isEnabled = true;
        public bool IsEnabled { get => isEnabled; set { isEnabled = value; OnPropertyChanged(); } }

        /// <summary>
        /// Creates a new instance of <see cref="NodeUIConfig"/> that is a copy of the current instance.
        /// </summary>
        /// <returns>A new <see cref="NodeUIConfig"/> instance with the same property values.</returns>
        public NodeUIConfig Clone()
        {
            return new NodeUIConfig
            {
                X = this.X,
                Y = this.Y,
                Name = this.Name,
                IsEnabled = this.IsEnabled
            };
        }

        public void CopyFrom(NodeUIConfig other)
        {
            x = other.X;
            y = other.Y;
            name = other.Name;
            isEnabled = other.IsEnabled;
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
