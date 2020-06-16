using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Data
{
    public class DataSelector : IBuildingSelector
    {
        private readonly string ConnectionString;
        public ConfigurationBuilder Builder { get; }

        internal DataSelector(string connectionString, ConfigurationBuilder builder)
        {
            this.ConnectionString = connectionString;
            this.Builder = builder;
        }
        public DataBuilder WithBlockBlobStorage(BlockBlobBuilder blockBlobBuilder)
        {
            blockBlobBuilder.DataConfiguration.ConnectionString = this.ConnectionString;
            return new DataBuilder(blockBlobBuilder.DataConfiguration, this);
        }
        public DataBuilder WithAppendBlobStorage(AppendBlobBuilder appendBlobBuilder)
        {
            appendBlobBuilder.DataConfiguration.ConnectionString = this.ConnectionString;
            return new DataBuilder(appendBlobBuilder.DataConfiguration, this);
        }
        public DataBuilder WithPageBlobStorage(PageBlobBuilder pageBlobBuilder)
        {
            pageBlobBuilder.DataConfiguration.ConnectionString = this.ConnectionString;
            return new DataBuilder(pageBlobBuilder.DataConfiguration, this);
        }
        public DataBuilder WithDataLakeV2(DataLakeV2Builder dataLakeV2Builder)
        {
            dataLakeV2Builder.DataConfiguration.ConnectionString = this.ConnectionString;
            return new DataBuilder(dataLakeV2Builder.DataConfiguration, this);
        }
    }
}
