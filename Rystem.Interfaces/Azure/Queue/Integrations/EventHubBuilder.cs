using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Queue
{
    public class EventHubBuilder
    {
        public QueueConfiguration QueueConfiguration { get; }
        public EventHubBuilder(string name)
        {
            this.QueueConfiguration = new QueueConfiguration()
            {
                Name = name,
                Type = QueueType.EventHub
            };
        }
    }
}
