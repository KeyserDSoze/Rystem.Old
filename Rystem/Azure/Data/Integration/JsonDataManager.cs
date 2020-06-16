using Newtonsoft.Json;
using Rystem.Const;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Data.Integration
{
    public class JsonDataManager<TEntity> : IDataReader<TEntity>, IDataWriter<TEntity>
        where TEntity : IData
    {
        public async Task<WrapperEntity<TEntity>> ReadAsync(DataWrapper dummy)
        {
            using (StreamReader sr = new StreamReader(dummy.Stream))
            {
                TEntity dataLake = (await sr.ReadToEndAsync().NoContext()).FromDefaultJson<TEntity>();
                dataLake.Properties = dummy.Properties;
                dataLake.Name = dummy.Name;
                return new WrapperEntity<TEntity>() { Entities = new List<TEntity>() { dataLake } };
            }
        }
        public async Task<DataWrapper> WriteAsync(TEntity entity)
        {
            await Task.Delay(0).NoContext();
            return new DataWrapper()
            {
                Properties = entity.Properties ?? new DataProperties() { ContentType = "text/json" },
                Name = entity.Name,
                Stream = new MemoryStream(Encoding.UTF8.GetBytes(entity.ToDefaultJson()))
            };
        }
    }
}
