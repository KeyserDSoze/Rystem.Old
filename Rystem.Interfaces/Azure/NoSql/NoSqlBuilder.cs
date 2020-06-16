using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.NoSql
{
    public class NoSqlBuilder : IInstallingBuilder
    {
        private readonly IConfiguration NoSqlConfiguration;
        private readonly NoSqlSelector NoSqlSelector;
        internal NoSqlBuilder(IConfiguration noSqlConfiguration, NoSqlSelector noSqlSelector)
        {
            this.NoSqlConfiguration = noSqlConfiguration;
            this.NoSqlSelector = noSqlSelector;
        }

        public InstallerType InstallerType => InstallerType.NoSql;

        public ConfigurationBuilder Build(Installation installation = Installation.Default)
        {
            this.NoSqlSelector.Builder.AddConfiguration(this.NoSqlConfiguration, this.InstallerType, installation);
            return this.NoSqlSelector.Builder;
        }
    }
}