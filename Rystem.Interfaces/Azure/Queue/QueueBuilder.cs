using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Queue
{
    public class QueueBuilder : IInstallingBuilder
    {
        private readonly IConfiguration QueueConfiguration;
        private readonly QueueSelector QueueChoser;
        internal QueueBuilder(IConfiguration queueConfiguration, QueueSelector queueChoser)
        {
            this.QueueConfiguration = queueConfiguration;
            this.QueueChoser = queueChoser;
        }
        public InstallerType InstallerType => InstallerType.Queue;
        public ConfigurationBuilder Build(Installation installation = Installation.Default)
        {
            this.QueueChoser.Builder.AddConfiguration(this.QueueConfiguration, this.InstallerType, installation);
            return this.QueueChoser.Builder;
        }
    }
}
