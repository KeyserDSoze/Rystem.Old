using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem
{
    public class TelemetrySelector : IBuildingSelector
    {
        public ConfigurationBuilder Builder { get; }
        private readonly string ConnectionString;
        internal TelemetrySelector(string connectionString, ConfigurationBuilder builder)
        {
            this.Builder = builder;
            this.ConnectionString = connectionString;
        }
        public TelemetryBuilder WithAppendBlob(AppendBlobTelemetryBuilder appendBlobBuilder)
        {
            appendBlobBuilder.TelemetryConfiguration.ConnectionString = this.ConnectionString;
            return new TelemetryBuilder(appendBlobBuilder.TelemetryConfiguration, this);
        }

        public TelemetryBuilder WithSql(SqlTelemetryBuilder sqlBuilder)
        {
            sqlBuilder.TelemetryConfiguration.ConnectionString = this.ConnectionString;
            return new TelemetryBuilder(sqlBuilder.TelemetryConfiguration, this);
        }
    }
}