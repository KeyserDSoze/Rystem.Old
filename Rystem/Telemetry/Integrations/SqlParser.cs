using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rystem.Aggregation;
using Rystem.Utility.SqlReflection;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rystem
{
    public static class SqlParserTest
    {
        private static SqlParser sqlParser;
        public static void SetSqlParser(string connectionString)
        {
            sqlParser = new SqlParser(new TelemetryConfiguration()
            {
                Name = "Combo",
                ConnectionString = connectionString,
                Type = TelemetryType.Sql
            });
        }
        public static async Task Set(IList<Telemetry> events)
        {
            await sqlParser.ParseAsync("sample", events, null, Installation.Default).NoContext();
        }
    }
    internal class SqlParser : IAggregationParser<Telemetry>
    {
        private readonly TelemetryConfiguration TelemetryConfiguration;
        private readonly string InsertQuery;
        private SqlConnection NewConnection() => new SqlConnection(this.TelemetryConfiguration.ConnectionString);
        private readonly SqlTable TelemetryDefault;
        private readonly SqlTable DependencyTelemetry;
        private bool IsFirstTime = true;
        public SqlParser(TelemetryConfiguration telemetryConfiguration)
        {
            this.TelemetryConfiguration = telemetryConfiguration;
            TelemetryDefault = new SqlTable($"Telemetry_{this.TelemetryConfiguration.Name}")
               .WithPrimaryKey("Id", SqlTablePrameterType.VarChar(64))
                .WithNotNullable("Start", SqlTablePrameterType.Datetime)
                .WithNotNullable("End", SqlTablePrameterType.Datetime)
                .WithNullable("Key", SqlTablePrameterType.VarChar(64))
                .WithNotNullable("ElapsedTime", SqlTablePrameterType.BigInt);
            DependencyTelemetry = new SqlTable($"Telemetry_Dependency_{this.TelemetryConfiguration.Name}").
               .WithPrimaryKey("Id", SqlTablePrameterType.VarChar(64))
               .WithPrimaryKey("Name", SqlTablePrameterType.VarChar(64))
               .WithPrimaryKey("Caller", SqlTablePrameterType.VarChar(64))
               .WithPrimaryKey("PathCaller", SqlTablePrameterType.VarChar(64))
               .WithPrimaryKey("LineNumberCaller", SqlTablePrameterType.VarChar(64))
               .WithPrimaryKey("Success", SqlTablePrameterType.VarChar(64));
        }

        public async Task ParseAsync(string queueName, IList<Telemetry> events, ILogger log, Installation installation)
        {
            List<Dictionary<string, object>> Defaults = new List<Dictionary<string, object>>();
            Dictionary<string, List<Dictionary<string, object>>> Others = new Dictionary<string, List<Dictionary<string, object>>>();
            foreach (Telemetry telemetry in events)
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("Id", telemetry.Id);
                parameters.Add("Start", telemetry.Start);
                parameters.Add("End", telemetry.End);
                parameters.Add("Key", telemetry.Key);
                parameters.Add("ElapsedTime", telemetry.ElapsedTime.Ticks);
                Defaults.Add(parameters);
                foreach (var telemetryByEvent in telemetry.Events.GroupBy(x => x.GetType().Name))
                {
                    if (!Others.ContainsKey(telemetryByEvent.Key))
                        Others.Add(telemetryByEvent.Key, new List<Dictionary<string, object>>());
                    foreach (var singleEvent in telemetryByEvent)
                        Others[telemetryByEvent.Key].Add(this.GetValues(singleEvent));
                }
            }
            using SqlConnection sqlConnection = this.NewConnection();
            if (IsFirstTime)
            {
                await this.TelemetryDefault.CreateIfNotExistsAsync(sqlConnection).NoContext();
                IsFirstTime = false;
            }
            await this.TelemetryDefault.InsertBulkAsync(sqlConnection, Defaults).NoContext();
        }
        private Dictionary<string, object> GetValues(ITelemetryEvent telemetryEvent)
        {
#warning Da risistemare per farlo diventare più veloce
            Dictionary<string, object> value = new Dictionary<string, object>();
            foreach (var property in telemetryEvent.GetType().GetProperties())
            {
                if (property.GetCustomAttribute(typeof(JsonIgnoreAttribute)) != null)
                    continue;
                value.Add(property.Name, property.GetValue(telemetryEvent));
            }
            return value;
        }
    }
    internal interface ISqlSaver<T>
        where T : ITelemetryEvent
    {
        Task Save(T singleEvent);
    }
}