using System;
using System.Collections.Generic;

namespace Rystem.Cache
{
    internal interface IMultitonManager
    {
        IMultiton Instance(IMultitonKey key);
        bool Update(IMultitonKey key, IMultiton value, TimeSpan expiringTime);
        bool Exists(IMultitonKey key);
        bool Delete(IMultitonKey key);
        IEnumerable<string> List();
    }
}
