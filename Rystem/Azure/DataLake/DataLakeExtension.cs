using Rystem.Azure.DataLake;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class DataLakeExtension
    {
        private static Dictionary<string, IDataLakeManager> Managers = new Dictionary<string, IDataLakeManager>();
        private readonly static object TrafficLight = new object();
        private static IDataLakeManager Manager(Type messageType)
        {
            if (!Managers.ContainsKey(messageType.FullName))
                lock (TrafficLight)
                    if (!Managers.ContainsKey(messageType.FullName))
                    {
                        Type genericType = typeof(DataLakeManager<>).MakeGenericType(messageType);
                        Managers.Add(messageType.FullName, (IDataLakeManager)Activator.CreateInstance(genericType));
                    }
            return Managers[messageType.FullName];
        }
        public static async Task<bool> AppendAsync<TEntity>(this TEntity entity)
            where TEntity : IDataLake
           => await Manager(entity.GetType()).AppendAsync(entity);
        public static async Task<string> WriteAsync<TEntity>(this TEntity entity)
        where TEntity : IDataLake
       => await Manager(entity.GetType()).WriteAsync(entity);
        public static async Task<bool> DeleteAsync<TEntity>(this TEntity entity)
            where TEntity : IDataLake
           => await Manager(entity.GetType()).DeleteAsync(entity);
        public static async Task<bool> ExistsAsync<TEntity>(this TEntity entity)
            where TEntity : IDataLake
           => await Manager(entity.GetType()).ExistsAsync(entity);
        public static async Task<IEnumerable<TEntity>> ListAsync<TEntity>(this TEntity entity, string prefix = null, int? takeCount = null)
            where TEntity : IDataLake
           => (await Manager(entity.GetType()).ListAsync(entity, prefix, takeCount)).Select(x => (TEntity)x);
        public static async Task<IList<string>> SearchAsync<TEntity>(this TEntity entity, string prefix = null, int? takeCount = null)
            where TEntity : IDataLake
           => (await Manager(entity.GetType()).SearchAsync(entity, prefix, takeCount));
    }
}
