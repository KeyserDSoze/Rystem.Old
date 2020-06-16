using Rystem.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Data
{
    public class DataBuilder : IInstallingBuilder
    {
        private readonly IConfiguration DataConfiguration;
        private readonly DataSelector DataSelector;
        internal DataBuilder(IConfiguration dataConfiguration, DataSelector dataSelector)
        {
            this.DataConfiguration = dataConfiguration;
            this.DataSelector = dataSelector;
        }

        public InstallerType InstallerType => InstallerType.Data;

        public ConfigurationBuilder Build(Installation installation = Installation.Default)
        {
            this.DataSelector.Builder.AddConfiguration(this.DataConfiguration, this.InstallerType, installation);
            return this.DataSelector.Builder;
        }
    }
}
