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
        private const string Query = "where Start >= @Start and Start <= @End";
        private const string QueryWithKey = "where Start >= @Start and Start <= @End and [Key] = @Key";
        private const string QueryInner = "where Timestamp >= @Start and Timestamp <= @End";
        private readonly List<string> ColumnNames = new List<string>();
        private readonly Type TelemetryType;
        public SqlTelemetry(SqlTelemetryConfiguration telemetryConfiguration, Telemetry telemetry)
        {
            this.TelemetryType = telemetry.GetType();
            this.TelemetryConfiguration = telemetryConfiguration;
            this.PrincipalTableName = $"Telemetry_{this.TelemetryConfiguration.Name}";

            Tables.Add(nameof(Telemetry), new SqlTable(PrincipalTableName)
                .AddType(telemetry.GetType())
                .With(LabelId, SqlTablePrameterType.VarChar(64))
                    .AsPrimaryKey()
                        .Compose<Telemetry>(x => x.Id, (x, y) => x.Id = y.ToString())
                .With("Key", SqlTablePrameterType.VarChar(64))
                    .Compose<Telemetry>(x => x.Key, (x, y) => x.Key = y.ToString())
                .With("Start", SqlTablePrameterType.Datetime)
                    .AsNotNullable()
                        .Compose<Telemetry>(x => x.Start, (x, y) => x.Start = (DateTime)y)
                .With("End", SqlTablePrameterType.Datetime)
                    .AsNotNullable()
                        .Compose<Telemetry>(x => x.End, (x, y) => x.End = (DateTime)y)
                .With("ElapsedTime", SqlTablePrameterType.BigInt)
                    .AsNotNullable()
                        .Compose<Telemetry>(x => x.Elapsed.Ticks));

            Tables.Add(nameof(DependencyTelemetry),
                CreateWithForeign<DependencyTelemetry>($"{PrincipalTableName}_Dependency")
               .With("Name", SqlTablePrameterType.VarChar(128))
                    .Compose<DependencyTelemetry>(x => x.Name, (x, y) => x.Name = y.ToString())
                .With("Caller", SqlTablePrameterType.VarChar(128))
                    .Compose<DependencyTelemetry>(x => x.Caller, (x, y) => x.Caller = y.ToString())
                .With("PathCaller", SqlTablePrameterType.VarChar(128))
                    .Compose<DependencyTelemetry>(x => x.PathCaller, (x, y) => x.PathCaller = y.ToString())
                .With("LineNumberCaller", SqlTablePrameterType.SmallInt)
                    .Compose<DependencyTelemetry>(x => x.LineNumberCaller, (x, y) => x.LineNumberCaller = (Int16)y)
                .With("Success", SqlTablePrameterType.Bit)
                    .Compose<DependencyTelemetry>(x => x.Success, (x, y) => x.Success = (bool)y)
                .With("Response", SqlTablePrameterType.Text)
                    .Compose<DependencyTelemetry>(x => x.Response, (x, y) => x.Response = y.ToString())
                .With("Elapsed", SqlTablePrameterType.BigInt)
                    .Compose<DependencyTelemetry>(x => x.Elapsed, (x, y) => x.Elapsed = (long)y));

            Tables.Add(nameof(ExceptionTelemetry),
                CreateWithForeign<ExceptionTelemetry>($"{PrincipalTableName}_Exception")
                .With("StackTrace", SqlTablePrameterType.Text)
                    .Compose<ExceptionTelemetry>(x => x.StackTrace, (x, y) => x.StackTrace = y.ToString())
                .With("HResult", SqlTablePrameterType.Int)
                    .Compose<ExceptionTelemetry>(x => x.HResult, (x, y) => x.HResult = (int)y));


            Tables.Add(nameof(MetricTelemetry), CreateWithForeign<MetricTelemetry>($"{PrincipalTableName}_Metric")
               .With("Name", SqlTablePrameterType.VarChar(64))
                    .Compose<MetricTelemetry>(x => x.Name, (x, y) => x.Name = y.ToString())
               .With("Value", SqlTablePrameterType.VarChar(128))
                    .Compose<MetricTelemetry>(x => x.Value, (x, y) => x.Value = y.ToString()));

            Tables.Add(nameof(TraceTelemetry), CreateWithForeign<TraceTelemetry>($"{PrincipalTableName}_Trace")
               .With("Message", SqlTablePrameterType.VarChar(256))
                    .Compose<TraceTelemetry>(x => x.Message, (x, y) => x.Message = y.ToString())
               .With("LogLevel", SqlTablePrameterType.SmallInt)
                    .Compose<TraceTelemetry>(x => (int)x.LogLevel, (x, y) => x.LogLevel = (LogLevel)(Int16)y));

            Tables.Add(nameof(RequestTelemetry), CreateWithForeign<RequestTelemetry>($"{PrincipalTableName}_Request")
               .With("Content", SqlTablePrameterType.Text)
                    .Compose<RequestTelemetry>(x => x.Content, (x, y) => x.Content = y.ToString())
               .With("Headers", SqlTablePrameterType.Text)
                    .Compose<RequestTelemetry>(x => x.Headers.ToDefaultJson(), (x, y) => x.Headers = y.ToString() == string.Empty ? new Dictionary<string, string>() : y.ToString().FromDefaultJson<Dictionary<string, string>>())
               .With("Method", SqlTablePrameterType.VarChar(10))
                    .Compose<RequestTelemetry>(x => x.Method, (x, y) => x.Method = y.ToString())
               .With("RequestUri", SqlTablePrameterType.VarChar(4096))
                    .Compose<RequestTelemetry>(x => x.RequestUri, (x, y) => x.RequestUri = y.ToString())
               .With("Version", SqlTablePrameterType.VarChar(20))
                    .Compose<RequestTelemetry>(x => x.Version, (x, y) => x.Version = y.ToString()));

            foreach (var table in telemetryConfiguration.CustomTables)
            {
                SqlTable customTable = CreateWithForeign<ITelemetryEvent>($"{PrincipalTableName}_Custom_{table.Value.Name}")
                    .AddType(table.Value.InstanceOf);
                foreach (var column in table.Value.Columns)
                    customTable.AddColumn(column);
                Tables.Add(table.Key, customTable);

            }

            SqlTable CreateWithForeign<TEntity>(string name)
                where TEntity : ITelemetryEvent
            {
                return new SqlTable(name)
                .AddType(typeof(TEntity))
                 .With(LabelId, SqlTablePrameterType.VarChar(64))
                    .AsPrimaryKey()
                        .Compose<TEntity>(x => Alea.GetTimedKey(), (x, y) => x.Id = y.ToString())
               .With(LabelForeignKey, SqlTablePrameterType.VarChar(64))
                    .AsNotNullable()
                        .WithForeignKey(PrincipalTableName, LabelId)
                            .Compose<TEntity>(x => x.Telemetry.Id, (x, y) => x.TelemetryId = y.ToString())
                .With("Timestamp", SqlTablePrameterType.Datetime)
                    .Compose<TEntity>(x => x.Timestamp, (x, y) => x.Timestamp = (DateTime)y);
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
            Dictionary<string, Telemetry> telemetries = new Dictionary<string, Telemetry>();
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@Start", from));
                parameters.Add(new SqlParameter("@End", to));
                if (key != null)
                    parameters.Add(new SqlParameter("@Key", key));
                using SqlConnection sqlConnection = this.NewConnection();
                await foreach (var telemetry in this.Tables.First().Value.GetAsync(sqlConnection, key != null ? QueryWithKey : Query, parameters))
                {
                    Telemetry dummyTelemetry = telemetry as Telemetry;
                    telemetries.Add(dummyTelemetry.Id, dummyTelemetry);
                }
                foreach (var table in this.Tables.Skip(1))
                {
                    List<SqlParameter> innerParameters = new List<SqlParameter>();
                    innerParameters.Add(new SqlParameter("@Start", from));
                    innerParameters.Add(new SqlParameter("@End", to));
                    await foreach (var possibleEvent in table.Value.GetAsync(sqlConnection, QueryInner, innerParameters))
                    {
                        ITelemetryEvent telemetryEvent = possibleEvent as ITelemetryEvent;
                        if (telemetries.ContainsKey(telemetryEvent.TelemetryId))
                        {
                            var telemetry = telemetries[telemetryEvent.TelemetryId];
                            telemetryEvent.Telemetry = telemetry;
                            telemetry.Events.Add(telemetryEvent);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string sol = ex.ToString();
            }
            return telemetries.Select(x => x.Value);
        }
    }
}