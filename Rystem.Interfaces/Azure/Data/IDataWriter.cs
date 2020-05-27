using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.Data
{
    public interface IDataWriter { }
    public interface IDataWriter<TEntity> : IDataWriter
        where TEntity : IData
    {
        Task<DataWrapper> WriteAsync(TEntity entity);
    }
}
