using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.Queue
{
    public class RijndaelBuilder
    {
        public QueueConfiguration QueueConfiguration { get; }
        public RijndaelBuilder(string name)
        {
            this.QueueConfiguration = new QueueConfiguration()
            {
                Name = name,
                Type = QueueType.EventHub
            };
        }
    }
}
