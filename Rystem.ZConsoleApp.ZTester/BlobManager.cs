using Rystem.Azure.AggregatedData;

namespace Reporting.WindTre.Library.Base.Blob
{
    public class BlobManager<TEntity> : IAggregatedDataReader<TEntity>, IAggregatedDataWriter<TEntity> where TEntity : BlobObject, new()
    {
        public TEntity Read(AggregatedDataDummy dummy)
        {
            return new TEntity()
            {
                Content = dummy.Stream,
                Name = dummy.Name,
                Properties = dummy.Properties
            };
        }

        public AggregatedDataDummy Write(TEntity entity)
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