using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.NoSql
{
    public class NoSqlSelector : IBuildingSelector
    {
        private readonly string ConnectionString;
        public ConfigurationBuilder Builder { get; }
        internal NoSqlSelector(string connectionString, ConfigurationBuilder builder)
        {
            this.ConnectionString = connectionString;
            this.Builder = builder;
        }
        public NoSqlBuilder WithTableStorage(TableStorageBuilder tableStorageBuilder)
        {
            tableStorageBuilder.NoSqlConfiguration.ConnectionString = this.ConnectionString;
            return new NoSqlBuilder(tableStorageBuilder.NoSqlConfiguration, this);
        }
        public NoSqlBuilder WithBlobStorage(BlobStorageBuilder blobStorageBuilder)
        {
            blobStorageBuilder.NoSqlConfiguration.ConnectionString = this.ConnectionString;
            return new NoSqlBuilder(blobStorageBuilder.NoSqlConfiguration, this);
        }
    }
}
