using Rystem.Cache;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Cache
{
    internal interface IMultitonIntegration<T>
    {
        T Instance(string key);
        bool Update(string key, T value, TimeSpan expiringTime);
        MultitonStatus<T> Exists(string key);
        bool Delete(string key);
        IEnumerable<string> List();
    }
}
