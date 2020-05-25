using System;
using System.Collections.Generic;

namespace Rystem
{
    /// <summary>
    /// Allows to start the configuration building through the WithInstallation method
    /// </summary>
    public class ConfigurationBuilder
    {
        public Dictionary<Installation, IConfiguration> Configurations { get; } = new Dictionary<Installation, IConfiguration>();
        /// <summary>
        /// Starts the configuration building
        /// </summary>
        /// <param name="installation">installation flow</param>
        /// <returns>Installer</returns>
        public Installer WithInstallation(Installation installation = Installation.Default)
            => new Installer(this, installation);
    }
}
