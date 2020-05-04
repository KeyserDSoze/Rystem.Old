using Rystem.Conversion;
using Rystem.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Rystem.Azure.AggregatedData.Integration
{
    public class FileDataManager<TEntity> : IAggregatedDataReader<TEntity>, IAggregatedDataWriter<TEntity>
          where TEntity : IFileData, new()
    {
        public async Task<AggregatedDataDummy> WriteAsync(TEntity entity)
        {
            var aggregatedDataDummy = new AggregatedDataDummy()
            {
                Properties = entity.Properties,
                Name = entity.Name,
            };
            await entity.Stream.CopyToAsync(aggregatedDataDummy.Stream).NoContext();
            return aggregatedDataDummy;
        }

        public async Task<TEntity> ReadAsync(AggregatedDataDummy dummy)
        {
            TEntity dataLake = new TEntity
            {
                Properties = dummy.Properties,
                Name = dummy.Name
            };
            await dummy.Stream.CopyToAsync((dataLake as IFileData).Stream).NoContext();
            return dataLake;
        }
    }
}
