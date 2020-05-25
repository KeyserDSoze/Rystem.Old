using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure
{
    internal class ManagerWrapper<TEntity>
    {
        internal readonly static Dictionary<string, IManager<TEntity>> Managers = new Dictionary<string, IManager<TEntity>>();
        internal readonly static object TrafficLight = new object();
    }
}
