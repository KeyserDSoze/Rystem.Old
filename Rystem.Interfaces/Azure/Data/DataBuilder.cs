using Rystem.Azure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.Data
{
    public class DataBuilder
    {
        private readonly IConfiguration DataConfiguration;
        private readonly DataSelector DataSelector;
        public DataBuilder(IConfiguration dataConfiguration, DataSelector dataSelector)
        {
            this.DataConfiguration = dataConfiguration;
            this.DataSelector = dataSelector;
        }
        public ConfigurationBuilder Build()
        {
            this.DataSelector.AzureInstaller.AddConfiguration(this.DataConfiguration);
            return this.DataSelector.AzureInstaller.Builder;
        }
    }
}
