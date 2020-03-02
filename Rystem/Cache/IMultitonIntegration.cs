using Rystem.Cache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Cache
{
    internal interface IMultitonIntegration<T> where T : IMultiton
    {
        T Instance(string key);
        bool Update(string key, T value, TimeSpan expiringTime);
        bool Exists(string key);
        bool Delete(string key);
        IEnumerable<string> List();
    }
}
