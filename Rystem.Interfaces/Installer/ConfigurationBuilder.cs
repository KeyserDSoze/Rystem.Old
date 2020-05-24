using System;
using System.Collections.Generic;

namespace Rystem
{
    public class ConfigurationBuilder
    {
        public Dictionary<Installation, IConfiguration> Configurations { get; } = new Dictionary<Installation, IConfiguration>();
        public Installer WithInstallation(Installation installation = Installation.Default)
            => new Installer(this, installation);
    }
}
