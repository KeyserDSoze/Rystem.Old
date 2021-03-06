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

namespace Rystem.Data.Integration
{
    public class FileDataManager<TEntity> : IDataReader<TEntity>, IDataWriter<TEntity>
          where TEntity : IFileData, new()
    {
        public async Task<DataWrapper> WriteAsync(TEntity entity)
        {
            var aggregatedDataDummy = new DataWrapper()
            {
                Properties = entity.Properties,
                Name = entity.Name,
            };
            MemoryStream ms = new MemoryStream();
            await entity.Stream.CopyToAsync(ms).NoContext();
            ms.Position = 0;
            aggregatedDataDummy.Stream = ms;
            return aggregatedDataDummy;
        }

        public async Task<WrapperEntity<TEntity>> ReadAsync(DataWrapper dummy)
        {
            TEntity dataLake = new TEntity
            {
                Properties = dummy.Properties,
                Name = dummy.Name
            };
            var fileData = (dataLake as IFileData);
            if (fileData.Stream == null)
                fileData.Stream = new MemoryStream();
            await dummy.Stream.CopyToAsync(fileData.Stream).NoContext();
            fileData.Stream.Position = 0;
            return new WrapperEntity<TEntity>() { Entities = new List<TEntity>() { dataLake } };
        }
    }
}
