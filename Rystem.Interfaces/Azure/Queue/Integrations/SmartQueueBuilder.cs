using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.Queue
{
    public class SmartQueueBuilder
    {
        public QueueConfiguration QueueConfiguration { get; }
        public SmartQueueBuilder(string name, QueueDuplication queueDuplication, int numberOfMessages = 100, int retry = 1, int retention = 30)
        {
            this.QueueConfiguration = new QueueConfiguration()
            {
                Name = name,
                CheckDuplication = queueDuplication,
                NumberOfMessages = numberOfMessages,
                Retry = retry,
                Retention = retention,
                Type = QueueType.SmartQueue
            };
        }
    }
}
