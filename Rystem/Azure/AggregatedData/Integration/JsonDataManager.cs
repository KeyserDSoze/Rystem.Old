using Newtonsoft.Json;
using Rystem.Const;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Azure.AggregatedData.Integration
{
    public class JsonDataManager<TEntity> : IAggregatedDataReader<TEntity>, IAggregatedDataWriter<TEntity>
        where TEntity : IAggregatedData, new()
    {
        public async Task<TEntity> ReadAsync(AggregatedDataDummy dummy)
        {
            using (StreamReader sr = new StreamReader(dummy.Stream))
            {
                TEntity dataLake = (await sr.ReadToEndAsync().NoContext()).FromStandardJson<TEntity>();
                dataLake.Properties = dummy.Properties;
                dataLake.Name = dummy.Name;
                return dataLake;
            }
        }
        public async Task<AggregatedDataDummy> WriteAsync(TEntity entity)
        {
            await Task.Delay(0).NoContext();
            return new AggregatedDataDummy()
            {
                Properties = entity.Properties ?? new AggregatedDataProperties() { ContentType = "text/json" },
                Name = entity.Name,
                Stream = new MemoryStream(Encoding.UTF8.GetBytes(entity.ToStandardJson()))
            };
        }
    }
}
