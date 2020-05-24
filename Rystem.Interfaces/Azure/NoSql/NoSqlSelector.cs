using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.NoSql
{
    public class NoSqlSelector
    {
        private readonly string ConnectionString;
        internal readonly Installer Installer;
        internal NoSqlSelector(string connectionString, Installer azureInstaller)
        {
            this.ConnectionString = connectionString;
            this.Installer = azureInstaller;
        }
        public NoSqlBuilder WithTableStorage(TableStorageBuilder tableStorageBuilder)
        {
            tableStorageBuilder.NoSqlConfiguration.ConnectionString = this.ConnectionString;
            return new NoSqlBuilder(tableStorageBuilder.NoSqlConfiguration, this);
        }
    }
}
