using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.Queue
{
    public class QueueStorageBuilder
    {
        public QueueConfiguration QueueConfiguration { get; }
        public QueueStorageBuilder(string name, int numberOfMessages = 100)
        {
            this.QueueConfiguration = new QueueConfiguration()
            {
                Name = name,
                NumberOfMessages = numberOfMessages,
                Type = QueueType.QueueStorage
            };
        }
    }
}
