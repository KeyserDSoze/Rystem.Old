using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rystem.Aggregation;
using Rystem.Utility;
using Rystem.Utility.SqlReflection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rystem
{
    internal class SqlTelemetry : IAggregationParser<Telemetry>, ITelemetryIntegration
    {
        private readonly TelemetryConfiguration TelemetryConfiguration;
        private SqlConnection NewConnection() => new SqlConnection(this.TelemetryConfiguration.ConnectionString);
        private readonly Dictionary<string, SqlTable> Tables = new Dictionary<string, SqlTable>();
        private const string LabelId = "Id";
        private const string LabelForeignKey = "TelemetryId";
        private readonly string PrincipalTableName;
        //private SqlTable GetSpecial(Type type)
        //{
        //    var sqlTable = new SqlTable($"{PrincipalTableName}_Custom_{type.Name}")
        //            .WithPrimaryKey(LabelId, SqlTablePrameterType.VarChar(64))
        //            .WithNullableForeignKey(LabelForeignKey, SqlTablePrameterType.VarChar(64), PrincipalTableName, LabelId);
        //    foreach (var property in type.FetchProperties(JsonIgnore))
        //        sqlTable.WithNullable(property.Name, SqlTablePrameterType.ByType(type));
        //    return sqlTable;
        //}
        private readonly string Query;
        private readonly string QueryWithKey;
        public SqlTelemetry(SqlTelemetryConfiguration telemetryConfiguration)
        {
            this.TelemetryConfiguration = telemetryConfiguration;
            this.PrincipalTableName = $"Telemetry_{this.TelemetryConfiguration.Name}";
            StringBuilder query = new StringBuilder();

            Tables.Add(nameof(Telemetry), new SqlTable(PrincipalTableName)
                .With(LabelId, SqlTablePrameterType.VarChar(64))
                    .AsPrimaryKey()
                        .Compose<Telemetry>(x => x.Id)
                .With("Key", SqlTablePrameterType.VarChar(64))
                    .Compose<Telemetry>(x => x.Key)
                .With("Start", SqlTablePrameterType.Datetime)
                    .AsNotNullable()
                        .Compose<Telemetry>(x => x.Start)
                .With("End", SqlTablePrameterType.Datetime)
                    .AsNotNullable()
                        .Compose<Telemetry>(x => x.End)
                .With("ElapsedTime", SqlTablePrameterType.BigInt)
                    .AsNotNullable()
                        .Compose<Telemetry>(x => x.Elapsed.Ticks));
            query.Append($"select * from {PrincipalTableName} T");

            Tables.Add(nameof(DependencyTelemetry), CreateWithForeign<DependencyTelemetry>($"{PrincipalTableName}_Dependency")
               .With("Name", SqlTablePrameterType.VarChar(128))
                    .Compose<DependencyTelemetry>(x => x.Name)
                .With("Caller", SqlTablePrameterType.VarChar(128))
                    .Compose<DependencyTelemetry>(x => x.Caller)
                .With("PathCaller", SqlTablePrameterType.VarChar(128))
                    .Compose<DependencyTelemetry>(x => x.PathCaller)
                .With("LineNumberCaller", SqlTablePrameterType.SmallInt)
                    .Compose<DependencyTelemetry>(x => x.LineNumberCaller)
                .With("Success", SqlTablePrameterType.Bit)
                    .Compose<DependencyTelemetry>(x => x.Success)
                .With("Response", SqlTablePrameterType.Text)
                    .Compose<DependencyTelemetry>(x => x.Response)
                .With("Elapsed", SqlTablePrameterType.BigInt)
                    .Compose<DependencyTelemetry>(x => x.Elapsed));
            query.Append($" inner join {PrincipalTableName}_Dependency D on T.{LabelId} = D.{LabelForeignKey}");

            Tables.Add(nameof(ExceptionTelemetry), CreateWithForeign<ExceptionTelemetry>($"{PrincipalTableName}_Exception")
                .With("StackTrace", SqlTablePrameterType.Text)
                    .Compose<ExceptionTelemetry>(x => x.StackTrace)
                .With("HResult", SqlTablePrameterType.Int)
                    .Compose<ExceptionTelemetry>(x => x.HResult));
            query.Append($" inner join {PrincipalTableName}_Exception E on T.{LabelId} = E.{LabelForeignKey}");

            Tables.Add(nameof(MetricTelemetry), CreateWithForeign<MetricTelemetry>($"{PrincipalTableName}_Metric")
               .With("Name", SqlTablePrameterType.VarChar(64))
                    .Compose<MetricTelemetry>(x => x.Name)
               .With("Value", SqlTablePrameterType.VarChar(128))
                    .Compose<MetricTelemetry>(x => x.Value));
            query.Append($" inner join {PrincipalTableName}_Metric M on T.{LabelId} = M.{LabelForeignKey}");

            Tables.Add(nameof(TraceTelemetry), CreateWithForeign<TraceTelemetry>($"{PrincipalTableName}_Trace")
               .With("Message", SqlTablePrameterType.VarChar(256))
                    .Compose<TraceTelemetry>(x => x.Message)
               .With("LogLevel", SqlTablePrameterType.SmallInt)
                    .Compose<TraceTelemetry>(x => (int)x.LogLevel));
            query.Append($" inner join {PrincipalTableName}_Trace C on T.{LabelId} = C.{LabelForeignKey}");

            Tables.Add(nameof(RequestTelemetry), CreateWithForeign<RequestTelemetry>($"{PrincipalTableName}_Request")
               .With("Content", SqlTablePrameterType.Text)
                    .Compose<RequestTelemetry>(x => x.Content)
               .With("Headers", SqlTablePrameterType.Text)
                    .Compose<RequestTelemetry>(x => x.Headers)
               .With("Method", SqlTablePrameterType.VarChar(10))
                    .Compose<RequestTelemetry>(x => x.Method)
               .With("RequestUri", SqlTablePrameterType.VarChar(4096))
                    .Compose<RequestTelemetry>(x => x.RequestUri)
               .With("Version", SqlTablePrameterType.VarChar(20))
                    .Compose<RequestTelemetry>(x => x.Version));
            query.Append($" inner join {PrincipalTableName}_Request R on T.{LabelId} = R.{LabelForeignKey}");

            foreach (var table in telemetryConfiguration.CustomTables)
            {
                SqlTable customTable = CreateWithForeign<ITelemetryEvent>($"{PrincipalTableName}_Custom_{table.Value.Name}");
                foreach (var column in table.Value.Columns)
                    customTable.AddColumn(column);
                Tables.Add(table.Key, customTable);
                query.Append($" inner join {PrincipalTableName}_Custom_{table.Value.Name} on T.{LabelId} = {PrincipalTableName}_Custom_{table.Value.Name}.{LabelForeignKey}");
            }
            query.Append(" where T.Start >= @Start");
            query.Append(" and T.Start <= @End");
            this.Query = query.ToString();
            query.Append(" and T.Key = @Key");
            this.QueryWithKey = query.ToString();

            SqlTable CreateWithForeign<TEntity>(string name)
                where TEntity : ITelemetryEvent
            {
                return new SqlTable(name)
                 .With(LabelId, SqlTablePrameterType.VarChar(64))
                    .AsPrimaryKey()
                        .Compose<TEntity>(x => Alea.GetTimedKey())
               .With(LabelForeignKey, SqlTablePrameterType.VarChar(64))
                    .AsNotNullable()
                        .WithForeignKey(PrincipalTableName, LabelId)
                            .Compose<TEntity>(x => x.Telemetry.Id)
                .With("Timestamp", SqlTablePrameterType.Datetime)
                    .Compose<TEntity>(x => x.Timestamp);
            }
        }
        private class Insertion
        {
            public bool HasTable => Tables.ContainsKey(this.SqlTableClassName);
            public SqlTable SqlTable => Tables[this.SqlTableClassName];
            public IEnumerable<object> Events { get; set; }
            public string SqlTableClassName { get; set; }
            public Dictionary<string, SqlTable> Tables { get; }
            public Insertion(Dictionary<string, SqlTable> tables)
                => this.Tables = tables;
            public async Task CreateIfNotExists(SqlConnection sqlConnection, ConcurrentDictionary<string, bool> created)
            {
                if (this.HasTable)
                {
                    if (!created.ContainsKey(this.SqlTableClassName) || !created[this.SqlTableClassName])
                    {
                        await this.SqlTable.CreateIfNotExistsAsync(sqlConnection, true).NoContext();
                        created.TryAdd(this.SqlTableClassName, true);
                    }
                }
            }
            public async Task BulkInsertAsync(SqlConnection sqlConnection)
                => await this.SqlTable.InsertBulkAsync(sqlConnection, this.Events).NoContext();
        }
        private readonly ConcurrentDictionary<string, bool> Created = new ConcurrentDictionary<string, bool>();
        public async Task ParseAsync(string queueName, IList<Telemetry> events, ILogger log, Installation installation)
        {
            Dictionary<string, Insertion> insertions = new Dictionary<string, Insertion>();
            Insertion telemetryInsertion = new Insertion(Tables) { SqlTableClassName = nameof(Telemetry), Events = events };
            foreach (Telemetry telemetry in events)
                foreach (var telemetryByEvent in telemetry.Events.GroupBy(x => x.GetType().Name))
                {
                    if (!insertions.ContainsKey(telemetryByEvent.Key))
                        insertions.Add(telemetryByEvent.Key, new Insertion(Tables) { Events = new List<object>(), SqlTableClassName = telemetryByEvent.Key });
                    (insertions[telemetryByEvent.Key].Events as List<object>).AddRange(telemetryByEvent);
                }
            try
            {
                using SqlConnection sqlConnection = this.NewConnection();
                await telemetryInsertion.CreateIfNotExists(sqlConnection, Created).NoContext();
                foreach (var insertion in insertions)
                    await insertion.Value.CreateIfNotExists(sqlConnection, Created).NoContext();
                await telemetryInsertion.BulkInsertAsync(sqlConnection).NoContext();
                List<Task> inserted = new List<Task>();
                foreach (var insertion in insertions)
                    inserted.Add(insertion.Value.BulkInsertAsync(sqlConnection));
                await Task.WhenAll(inserted);
            }
            catch (Exception ex)
            {
                string olaf = ex.ToString();
            }
        }

        public async Task<IEnumerable<Telemetry>> GetEventsAsync(DateTime from, DateTime to, string key)
        {
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@Start", from));
                parameters.Add(new SqlParameter("@End", to));
                if (key != null)
                    parameters.Add(new SqlParameter("@Key", key));
                using SqlConnection sqlConnection = this.NewConnection();
                await foreach (var reader in this.Tables.First().Value.GetAsync(sqlConnection, key != null ? this.QueryWithKey : this.Query, parameters))
                {
                    var t = reader;
                }
            }
            catch (Exception ex)
            {
                string sol = ex.ToString();
            }
            return null;
        }
    }
}