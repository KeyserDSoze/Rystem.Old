using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Queue
{
    public class ServiceBusBuilder
    {
        public QueueConfiguration QueueConfiguration { get; }
        public ServiceBusBuilder(string name)
        {
            this.QueueConfiguration = new QueueConfiguration()
            {
                Name = name,
                Type = QueueType.ServiceBus
            };
        }
    }
}
