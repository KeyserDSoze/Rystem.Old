using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.Queue
{
    public class QueueBuilder
    {
        private readonly IConfiguration QueueConfiguration;
        private readonly QueueSelector QueueChoser;
        public QueueBuilder(IConfiguration queueConfiguration, QueueSelector queueChoser)
        {
            this.QueueConfiguration = queueConfiguration;
            this.QueueChoser = queueChoser;
        }
        public ConfigurationBuilder Build()
        {
            this.QueueChoser.AzureInstaller.AddConfiguration(this.QueueConfiguration);
            return this.QueueChoser.AzureInstaller.Builder;
        }
    }
}
