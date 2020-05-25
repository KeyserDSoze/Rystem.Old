using Rystem.Azure.NoSql;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure
{
    internal static class ManagerExtensions
    {
        internal static IManager<TEntity> DefaultManager<TEntity>(this TEntity entity, Func<TEntity, IManager<TEntity>> managerCreator) 
            => DefaultManager<TEntity, TEntity>(entity, managerCreator);
        internal static IManager<TEntity> DefaultManager<TWrapper, TEntity>(this TWrapper entity, Func<TWrapper, IManager<TEntity>> managerCreator)
        {
            Type entityType = entity.GetType();
            if (!ManagerWrapper<TEntity>.Managers.ContainsKey(entityType.FullName))
                lock (ManagerWrapper<TEntity>.TrafficLight)
                    if (!ManagerWrapper<TEntity>.Managers.ContainsKey(entityType.FullName))
                    {
                        ManagerWrapper<TEntity>.Managers.Add(entityType.FullName, managerCreator.Invoke(entity));
                    }
            return ManagerWrapper<TEntity>.Managers[entityType.FullName];
        }
    }
}
