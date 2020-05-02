using System.Collections.Generic;
using Rystem.Crypting;

namespace System
{
    public static class CryptoExtensions
    {
        private static Dictionary<string, ICryptoManager> Managers = new Dictionary<string, ICryptoManager>();
        private readonly static object TrafficLight = new object();
        private static ICryptoManager Manager(Type messageType)
        {
            if (!Managers.ContainsKey(messageType.FullName))
                lock (TrafficLight)
                    if (!Managers.ContainsKey(messageType.FullName))
                    {
                        Type genericType = typeof(CryptoManager<>).MakeGenericType(messageType);
                        Managers.Add(messageType.FullName, (ICryptoManager)Activator.CreateInstance(genericType));
                    }
            return Managers[messageType.FullName];
        }
        public static TEntity Encrypt<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : ICrypto
           => Manager(entity.GetType()).Encrypt(entity, installation);
        public static TEntity Decrypt<TEntity>(this TEntity entity, Installation installation = Installation.Default)
            where TEntity : ICrypto
           => Manager(entity.GetType()).Decrypt(entity, installation);
    }
}
