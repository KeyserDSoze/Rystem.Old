using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.Queue
{
    internal class SmartMessage<TEntity>
        where TEntity : IQueue
    {
        public int Path { get; set; }
        public int Organization { get; set; }
        public TEntity Message { get; set; }
    }
}
