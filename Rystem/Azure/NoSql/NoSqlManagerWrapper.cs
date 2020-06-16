using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.NoSql
{
    internal class ManagerWrapper<TEntity>
    {
        internal readonly static Dictionary<string, INoSqlManager<TEntity>> Managers = new Dictionary<string, INoSqlManager<TEntity>>();
        internal readonly static object TrafficLight = new object();
    }
}
