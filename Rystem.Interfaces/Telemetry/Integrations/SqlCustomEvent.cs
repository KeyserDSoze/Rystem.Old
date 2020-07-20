using Rystem.Utility;
using Rystem.Utility.SqlReflection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem
{
    public class SqlCustomEvent
    {
        private readonly TelemetryBuilder TelemetryBuilder;
        public SqlCustomEvent(TelemetryBuilder telemetryBuilder)
            => this.TelemetryBuilder = telemetryBuilder;
        public SqlCustomEvent AddCustomEvent<TEntity>(SqlTable sqlTable)
        {
            sqlTable.AddType(typeof(TEntity));
            (this.TelemetryBuilder.TelemetryConfiguration as SqlTelemetryConfiguration).CustomTables.Add(typeof(TEntity).Name, sqlTable);
            return this;
        }
        public ConfigurationBuilder Build(Installation installation = Installation.Default)
            => TelemetryBuilder.Build(installation);
    }
}