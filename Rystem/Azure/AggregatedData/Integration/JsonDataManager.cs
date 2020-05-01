using Newtonsoft.Json;
using Rystem.Const;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.AggregatedData.Integration
{
    internal class JsonDataManager<TEntity> : IAggregatedDataReader<TEntity>, IAggregatedDataWriter<TEntity>
        where TEntity : IAggregatedData
    {
        public async Task<TEntity> ReadAsync(AggregatedDataDummy dummy)
        {
            using (StreamReader sr = new StreamReader(dummy.Stream))
            {
                TEntity dataLake = JsonConvert.DeserializeObject<TEntity>(await sr.ReadToEndAsync(), NewtonsoftConst.AutoNameHandling_NullIgnore_JsonSettings);
                dataLake.Properties = dummy.Properties;
                dataLake.Name = dummy.Name;
                return dataLake;
            }
        }
        public async Task<AggregatedDataDummy> WriteAsync(TEntity entity)
        {
            return new AggregatedDataDummy()
            {
                Properties = entity.Properties ?? new AggregatedDataProperties() { ContentType = "text/json" },
                Name = entity.Name,
                Stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(entity, NewtonsoftConst.AutoNameHandling_NullIgnore_JsonSettings)))
            };
        }
    }
}
