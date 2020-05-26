using Rystem.Cache;
using System;
using System.Collections.Generic;

namespace Rystem
{
    /// <summary>
    /// Allows to start the configuration building through the WithInstallation method
    /// </summary>
    public class ConfigurationBuilder
    {
        internal Dictionary<InstallerType, Dictionary<Installation, IConfiguration>> Configurations { get; } = new Dictionary<InstallerType, Dictionary<Installation, IConfiguration>>();
        /// <summary>
        /// Starts the configuration building
        /// </summary>
        /// <param name="installation">installation flow</param>
        /// <returns>Installer</returns>
        public Installer WithInstallation(Installation installation = Installation.Default)
            => new Installer(this, installation);
        //public CacheBuilder WithCache(CacheConsistency cacheConsistency = CacheConsistency.Always)
        //    => new CacheBuilder(cacheConsistency);
        public Dictionary<Installation, IConfiguration> GetConfigurations(InstallerType installerType)
            => this.Configurations[installerType];
    }
}
