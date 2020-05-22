using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.Data
{
    public class WrapperEntity<TEntity>
        where TEntity : IData
    {
        public List<TEntity> Entities { get; set; }
    }
    public interface IDataReader<TEntity>
        where TEntity : IData
    {
        Task<WrapperEntity<TEntity>> ReadAsync(DataWrapper dummy);
    }
}
