using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Crypting
{
    internal interface ICryptoManager
    {
        TEntity Encrypt<TEntity>(TEntity entity, Installation installation) where TEntity : ICrypto;
        TEntity Decrypt<TEntity>(TEntity entity, Installation installation) where TEntity : ICrypto;
    }
}
