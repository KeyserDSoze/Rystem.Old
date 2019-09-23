using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rystem.Azure.AggregatedData.Integration
{
    internal class JsonDataManager<TEntity> : IAggregatedDataReader<TEntity>, IDataLakeWriter
        where TEntity : IAggregatedData
    {
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore
        };
        public TEntity Read(AggregatedDataDummy dummy)
        {
            using (StreamReader sr = new StreamReader(dummy.Stream))
            {
                TEntity dataLake = JsonConvert.DeserializeObject<TEntity>(sr.ReadToEnd(), JsonSettings);
                dataLake.Properties = dummy.Properties;
                dataLake.Name = dummy.Name;
                return dataLake;
            }
        }
        public AggregatedDataDummy Write(IAggregatedData entity)
        {
            return new AggregatedDataDummy()
            {
                Properties = entity.Properties ?? new AggregatedDataProperties() { ContentType = "text/json" },
                Name = entity.Name,
                Stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(entity, JsonSettings)))
            };
        }
    }
}
