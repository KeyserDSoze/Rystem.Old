using System;
using System.Collections.Generic;

namespace Rystem.Cache
{
    internal interface IMultitonManager
    {
        IMultiton Instance(IMultitonKey key);
        TEntry Instance<TEntry>(IMultitonKey<TEntry> key) where TEntry : IMultiton, new();
        bool Update(IMultitonKey key, IMultiton value, TimeSpan expiringTime);
        bool Update<TEntry>(IMultitonKey<TEntry> key, TEntry value, TimeSpan expiringTime) where TEntry : IMultiton, new();
        bool Exists(IMultiKey key);
        bool Delete(IMultiKey key);
        IEnumerable<string> List();
    }
}
