using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Data.Integration
{
    internal class AvroDataManager<TEntity> : IDataReader<TEntity>, IDataWriter<TEntity>
          where TEntity : IData
    {
        public AvroDataManager()
        {
        }
        public async Task<WrapperEntity<TEntity>> ReadAsync(DataWrapper dummy)
        {
            List<TEntity> aggregatedDatas = new List<TEntity>();
            using (StreamReader sr = new StreamReader(dummy.Stream))
            {
                while (!sr.EndOfStream)
                {
                    TEntity aggregatedData = (await sr.ReadLineAsync().NoContext()).FromDefaultJson<TEntity>();
                    aggregatedData.Properties = dummy.Properties;
                    aggregatedData.Name = dummy.Name;
                    aggregatedDatas.Add(aggregatedData);
                }
            }
            return new WrapperEntity<TEntity>() { Entities = aggregatedDatas };
        }

        public async Task<DataWrapper> WriteAsync(TEntity entity)
        {
            await Task.Delay(0).NoContext();
            return new DataWrapper()
            {
                Properties = entity.Properties ?? new DataProperties() { ContentType = "application/avro" },
                Name = entity.Name,
                Stream = new MemoryStream(Encoding.UTF8.GetBytes(entity.ToDefaultJson()))
            };
        }
    }
}