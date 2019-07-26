using Rystem.Cache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    internal abstract class AMultitonIntegration<T> where T : IMultiton
    {
        internal abstract T Instance(string key);
        internal abstract bool Update(string key, T value);
        internal abstract bool Exists(string key);
        internal abstract bool Delete(string key);
        internal abstract IEnumerable<string> List();
    }
    internal abstract class AMultitonManager
    {
        internal abstract IMultiton Get(IMultitonKey key);
        internal abstract IMultiton Instance(IMultitonKey key);
        internal abstract bool Update(IMultitonKey key, IMultiton value);
        internal abstract bool Exists(IMultitonKey key);
        internal abstract bool Delete(IMultitonKey key);
        internal abstract IEnumerable<string> List();
    }
}
