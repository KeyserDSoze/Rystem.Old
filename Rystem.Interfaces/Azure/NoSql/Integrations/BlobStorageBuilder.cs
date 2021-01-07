using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.NoSql
{
    public class BlobStorageBuilder
    {
        public NoSqlConfiguration NoSqlConfiguration { get; }
        public BlobStorageBuilder()
        {
            this.NoSqlConfiguration = new NoSqlConfiguration()
            {
                Type = NoSqlType.BlobStorage
            };
        }
        public BlobStorageBuilder(string name)
        {
            this.NoSqlConfiguration = new NoSqlConfiguration()
            {
                Name = name,
                Type = NoSqlType.BlobStorage
            };
        }
    }
}