using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem
{
    internal class ManagerWrapper<TEntity>
    {
        internal readonly static Dictionary<string, IRystemManager<TEntity>> Managers = new Dictionary<string, IRystemManager<TEntity>>();
        internal readonly static object TrafficLight = new object();
    }
}