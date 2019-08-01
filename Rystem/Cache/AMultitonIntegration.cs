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
    internal interface IMultitonManager
    {
        IMultiton Instance(IMultitonKey key);
        bool Update(IMultitonKey key, IMultiton value);
        bool Exists(IMultitonKey key);
        bool Delete(IMultitonKey key);
        IEnumerable<string> List();
    }
}
