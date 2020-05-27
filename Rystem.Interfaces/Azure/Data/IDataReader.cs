using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.Data
{
    public interface IDataReader { }
    public interface IDataReader<TEntity> : IDataReader
        where TEntity : IData
    {
        Task<WrapperEntity<TEntity>> ReadAsync(DataWrapper dummy);
    }
}
