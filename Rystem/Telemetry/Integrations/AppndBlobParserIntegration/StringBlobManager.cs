using Rystem.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem
{
    internal class StringBlobManager<T, TEntity> : IDataWriter<T>, IDataReader<T>
        where T : ITelemetryData<TEntity>, new()
    {
        public async Task<WrapperEntity<T>> ReadAsync(DataWrapper dummy)
        {
            List<TEntity> telemetries = new List<TEntity>();
            using StreamReader reader = new StreamReader(dummy.Stream);
            while (!reader.EndOfStream)
                telemetries.Add((await reader.ReadLineAsync()).FromDefaultJson<TEntity>());
            return new WrapperEntity<T>()
            {
                Entities = new List<T> { new T { Name = dummy.Name, Properties = dummy.Properties, Events = telemetries } }
            };
        }

        public Task<DataWrapper> WriteAsync(T entity)
        {
            return Task.FromResult(new DataWrapper()
            {
                Properties = entity.Properties,
                Name = entity.Name,
                Stream = new MemoryStream(Encoding.UTF8.GetBytes($"{string.Join('\n', entity.Events.Select(x => x.ToDefaultJson()))}{'\n'}"))
            });
        }
    }
}
