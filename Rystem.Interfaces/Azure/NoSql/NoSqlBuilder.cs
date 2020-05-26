using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.NoSql
{
    public class NoSqlBuilder : IBuilder
    {
        private readonly IConfiguration NoSqlConfiguration;
        private readonly NoSqlSelector NoSqlSelector;
        internal NoSqlBuilder(IConfiguration noSqlConfiguration, NoSqlSelector noSqlSelector)
        {
            this.NoSqlConfiguration = noSqlConfiguration;
            this.NoSqlSelector = noSqlSelector;
        }

        public InstallerType InstallerType => InstallerType.NoSql;

        public ConfigurationBuilder Build()
        {
            this.NoSqlSelector.Installer.AddConfiguration(this.NoSqlConfiguration, this.InstallerType);
            return this.NoSqlSelector.Installer.Builder;
        }
    }
}
