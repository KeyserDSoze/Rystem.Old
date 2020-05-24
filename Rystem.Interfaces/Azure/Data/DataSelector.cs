using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.Data
{
    public class DataSelector
    {
        private readonly string ConnectionString;
        internal readonly Installer AzureInstaller;
        internal DataSelector(string connectionString, Installer azureInstaller)
        {
            this.ConnectionString = connectionString;
            this.AzureInstaller = azureInstaller;
        }
        public DataBuilder WithBlockBlobStorage<TData>(BlockBlobBuilder<TData> blockBlobBuilder)
            where TData : IData
        {
            blockBlobBuilder.DataConfiguration.ConnectionString = this.ConnectionString;
            return new DataBuilder(blockBlobBuilder.DataConfiguration, this);
        }
        public DataBuilder WithAppendBlobStorage<TData>(AppendBlobBuilder<TData> appendBlobBuilder)
            where TData : IData
        {
            appendBlobBuilder.DataConfiguration.ConnectionString = this.ConnectionString;
            return new DataBuilder(appendBlobBuilder.DataConfiguration, this);
        }
        public DataBuilder WithPageBlobStorage<TData>(PageBlobBuilder<TData> pageBlobBuilder)
            where TData : IData
        {
            pageBlobBuilder.DataConfiguration.ConnectionString = this.ConnectionString;
            return new DataBuilder(pageBlobBuilder.DataConfiguration, this);
        }
        public DataBuilder WithDataLakeV2<TData>(DataLakeV2Builder<TData> dataLakeV2Builder)
            where TData : IData
        {
            dataLakeV2Builder.DataConfiguration.ConnectionString = this.ConnectionString;
            return new DataBuilder(dataLakeV2Builder.DataConfiguration, this);
        }
    }
}
