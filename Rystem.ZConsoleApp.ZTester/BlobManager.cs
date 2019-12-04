using Rystem.Azure.AggregatedData;
using System.Threading.Tasks;

namespace Reporting.WindTre.Library.Base.Blob
{
    public class BlobManager<TEntity> : IAggregatedDataReader<TEntity>, IAggregatedDataWriter<TEntity> where TEntity : BlobObject, new()
    {
        public async Task<TEntity> ReadAsync(AggregatedDataDummy dummy)
        {
            return new TEntity()
            {
                Content = dummy.Stream,
                Name = dummy.Name,
                Properties = dummy.Properties
            };
        }

        public async Task<AggregatedDataDummy> WriteAsync(TEntity entity)
        {
            return new AggregatedDataDummy()
            {
                Name = entity.Name,
                Properties = entity.Properties,
                Stream = entity.Content
            };
        }
    }
}