using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.Queue
{
    public class QueueBuilder : IBuilder
    {
        private readonly IConfiguration QueueConfiguration;
        private readonly QueueSelector QueueChoser;
        internal QueueBuilder(IConfiguration queueConfiguration, QueueSelector queueChoser)
        {
            this.QueueConfiguration = queueConfiguration;
            this.QueueChoser = queueChoser;
        }
        public InstallerType InstallerType => InstallerType.Queue;
        public ConfigurationBuilder Build()
        {
            this.QueueChoser.AzureInstaller.AddConfiguration(this.QueueConfiguration, this.InstallerType);
            return this.QueueChoser.AzureInstaller.Builder;
        }
    }
}
