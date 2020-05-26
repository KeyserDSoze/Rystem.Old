using Rystem.Azure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.Data
{
    public class DataBuilder : IBuilder
    {
        private readonly IConfiguration DataConfiguration;
        private readonly DataSelector DataSelector;
        internal DataBuilder(IConfiguration dataConfiguration, DataSelector dataSelector)
        {
            this.DataConfiguration = dataConfiguration;
            this.DataSelector = dataSelector;
        }

        public InstallerType InstallerType => InstallerType.Data;

        public ConfigurationBuilder Build()
        {
            this.DataSelector.AzureInstaller.AddConfiguration(this.DataConfiguration, this.InstallerType);
            return this.DataSelector.AzureInstaller.Builder;
        }
    }
}
