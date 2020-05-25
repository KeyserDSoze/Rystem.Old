using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.NoSql
{
    public class NoSqlBuilder
    {
        private readonly IConfiguration NoSqlConfiguration;
        private readonly NoSqlSelector NoSqlSelector;
        internal NoSqlBuilder(IConfiguration noSqlConfiguration, NoSqlSelector noSqlSelector)
        {
            this.NoSqlConfiguration = noSqlConfiguration;
            this.NoSqlSelector = noSqlSelector;
        }
        public ConfigurationBuilder Build()
        {
            this.NoSqlSelector.Installer.AddConfiguration(this.NoSqlConfiguration);
            return this.NoSqlSelector.Installer.Builder;
        }
    }
}
