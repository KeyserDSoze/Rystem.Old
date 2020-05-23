using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    internal interface IInMemoryInstance
    {
        long ExpiringTime { get; set; }
    }
    internal class InMemoryInstance<T> : IInMemoryInstance
    {
        public T Instance { get; set; }
        public long ExpiringTime { get; set; }
    }
}
