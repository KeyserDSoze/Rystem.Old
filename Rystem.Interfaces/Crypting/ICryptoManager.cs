using System;

namespace Rystem.Crypting
{
    internal interface ICryptoManager : IRystemManager
    {
        TEntity Encrypt<TEntity>(TEntity entity, Installation installation) where TEntity : ICrypto;
        TEntity Decrypt<TEntity>(TEntity entity, Installation installation) where TEntity : ICrypto;
    }
}
