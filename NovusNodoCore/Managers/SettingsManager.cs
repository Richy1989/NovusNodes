using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NovusNodoCore.Managers
{
    /// <summary>
    /// Manages application settings.
    /// </summary>
    public class SettingsManager
    {
        private readonly ILogger<SettingsManager> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsManager"/> class.
        /// </summary>
        /// <param name="logger">The logger instance to use for logging.</param>
        public SettingsManager(ILogger<SettingsManager> logger)
        {
            _logger = logger;
        }
    }
}
