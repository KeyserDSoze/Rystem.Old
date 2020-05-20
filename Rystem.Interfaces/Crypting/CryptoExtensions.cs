using System.Collections.Generic;
using Rystem.Crypting;

namespace System
{
    public static partial class CryptoExtensions
    {
        private readonly static Dictionary<string, ICryptoManager> Managers = new Dictionary<string, ICryptoManager>();
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
    public static partial class CryptoExtensions
    {
        public static string DefaultEncrypt(string entry)
            => new DefaultCrypto() { Message = entry }.Encrypt(Installation.Default).CryptedMessage;
        public static string DefaultDecrypt(this string entry)
            => new DefaultCrypto() { CryptedMessage = entry }.Decrypt(Installation.Default).Message;
        public static string DefaultEncrypt<TEntity>(this TEntity entity)
            => new DefaultCrypto() { Message = entity.ToDefaultJson() }.Encrypt(Installation.Default).CryptedMessage;
        public static TEntity DefaultDecrypt<TEntity>(this string entry)
            => new DefaultCrypto() { CryptedMessage = entry }.Decrypt(Installation.Default).Message.FromDefaultJson<TEntity>();
        public static string DefaultHash<TEntity>(this TEntity entity)
            => new DefaultCrypto() { Message = entity.ToDefaultJson() }.Encrypt(Installation.Inst00).CryptedMessage;
    }
}