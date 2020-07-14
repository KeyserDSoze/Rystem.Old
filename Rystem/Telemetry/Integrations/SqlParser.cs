using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rystem.Aggregation;
using Rystem.Utility;
using Rystem.Utility.SqlReflection;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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
        private SqlConnection NewConnection() => new SqlConnection(this.TelemetryConfiguration.ConnectionString);
        private readonly SqlTable TelemetryDefault;
        private readonly SqlTable Dependency;
        private readonly SqlTable Exception;
        private readonly SqlTable Metric;
        private readonly SqlTable Trace;
        private readonly SqlTable Request;
        private bool IsFirstTime = true;
        private const string LabelId = "Id";
        private const string LabelForeignKey = "TelemetryId";
        private readonly string PrincipalTableName;
        private SqlTable GetSpecial(Type type)
        {
            var sqlTable = new SqlTable($"{PrincipalTableName}_Custom_{type.Name}")
                    .WithPrimaryKey(LabelId, SqlTablePrameterType.VarChar(64))
                    .WithNullableForeignKey(LabelForeignKey, SqlTablePrameterType.VarChar(64), PrincipalTableName, LabelId)
                    .WithNullable("Timestamp", SqlTablePrameterType.Datetime);
            foreach (var property in type.FetchProperties(JsonIgnore))
                sqlTable.WithNullable(property.Name, SqlTablePrameterType.ByType(type));
            return sqlTable;
        }

        public SqlParser(TelemetryConfiguration telemetryConfiguration)
        {
            this.TelemetryConfiguration = telemetryConfiguration;
            this.PrincipalTableName = $"Telemetry_{this.TelemetryConfiguration.Name}";
            TelemetryDefault = new SqlTable(PrincipalTableName)
               .WithPrimaryKey(LabelId, SqlTablePrameterType.VarChar(64))
                .WithNullable("Key", SqlTablePrameterType.VarChar(64))
                .WithNotNullable("Start", SqlTablePrameterType.Datetime)
                .WithNotNullable("End", SqlTablePrameterType.Datetime)
                .WithNotNullable("ElapsedTime", SqlTablePrameterType.BigInt);
            Dependency = new SqlTable($"{PrincipalTableName}_Dependency")
               .WithPrimaryKey(LabelId, SqlTablePrameterType.VarChar(64))
               .WithNullableForeignKey(LabelForeignKey, SqlTablePrameterType.VarChar(64), PrincipalTableName, LabelId)
               .WithNullable("Name", SqlTablePrameterType.VarChar(128))
               .WithNullable("Caller", SqlTablePrameterType.VarChar(128))
               .WithNullable("PathCaller", SqlTablePrameterType.VarChar(128))
               .WithNullable("LineNumberCaller", SqlTablePrameterType.SmallInt)
               .WithNullable("Success", SqlTablePrameterType.Bit)
               .WithNullable("Response", SqlTablePrameterType.Text)
               .WithNullable("Timestamp", SqlTablePrameterType.Datetime)
               .WithNullable("Elapsed", SqlTablePrameterType.BigInt);
            Exception = new SqlTable($"{PrincipalTableName}_Exception")
               .WithPrimaryKey(LabelId, SqlTablePrameterType.VarChar(64))
               .WithNullableForeignKey(LabelForeignKey, SqlTablePrameterType.VarChar(64), PrincipalTableName, LabelId)
               .WithNullable("Timestamp", SqlTablePrameterType.Datetime)
               .WithNullable("StackTrace", SqlTablePrameterType.Text)
               .WithNullable("HResult", SqlTablePrameterType.Int);
            Metric = new SqlTable($"{PrincipalTableName}_Metric")
               .WithPrimaryKey(LabelId, SqlTablePrameterType.VarChar(64))
               .WithNullableForeignKey(LabelForeignKey, SqlTablePrameterType.VarChar(64), PrincipalTableName, LabelId)
               .WithNullable("Timestamp", SqlTablePrameterType.Datetime)
               .WithNullable("Name", SqlTablePrameterType.VarChar(64))
               .WithNullable("Value", SqlTablePrameterType.VarChar(128));
            Trace = new SqlTable($"{PrincipalTableName}_Trace")
               .WithPrimaryKey(LabelId, SqlTablePrameterType.VarChar(64))
               .WithNullableForeignKey(LabelForeignKey, SqlTablePrameterType.VarChar(64), PrincipalTableName, LabelId)
               .WithNullable("Timestamp", SqlTablePrameterType.Datetime)
               .WithNullable("Message", SqlTablePrameterType.VarChar(256))
               .WithNullable("LogLevel", SqlTablePrameterType.SmallInt);
            Request = new SqlTable($"{PrincipalTableName}_Request")
               .WithPrimaryKey(LabelId, SqlTablePrameterType.VarChar(64))
               .WithNullableForeignKey(LabelForeignKey, SqlTablePrameterType.VarChar(64), PrincipalTableName, LabelId)
               .WithNullable("Timestamp", SqlTablePrameterType.Datetime)
               .WithNullable("Content", SqlTablePrameterType.Text)
               .WithNullable("Headers", SqlTablePrameterType.Text)
               .WithNullable("Method", SqlTablePrameterType.VarChar(10))
               .WithNullable("RequestUri", SqlTablePrameterType.VarChar(4096))
               .WithNullable("Version", SqlTablePrameterType.VarChar(20));
        }
        private readonly RaceCondition RaceCondition = new RaceCondition();
        public async Task ParseAsync(string queueName, IList<Telemetry> events, ILogger log, Installation installation)
        {
            List<Dictionary<string, object>> Defaults = new List<Dictionary<string, object>>();
            Dictionary<Type, List<Dictionary<string, object>>> Others = new Dictionary<Type, List<Dictionary<string, object>>>();
            foreach (Telemetry telemetry in events)
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { LabelId, telemetry.Id },
                    { "Start", telemetry.Start },
                    { "End", telemetry.End },
                    { "Key", telemetry.Key },
                    { "ElapsedTime", telemetry.ElapsedTime }
                };
                Defaults.Add(parameters);
                foreach (var telemetryByEvent in telemetry.Events.GroupBy(x => x.GetType()))
                {
                    var properties = telemetryByEvent.Key.FetchProperties(JsonIgnore);
                    if (!Others.ContainsKey(telemetryByEvent.Key))
                        Others.Add(telemetryByEvent.Key, new List<Dictionary<string, object>>());
                    foreach (var singleEvent in telemetryByEvent)
                        Others[telemetryByEvent.Key].Add(this.GetValues(singleEvent, telemetry.Id, properties));
                }
            }
            using SqlConnection sqlConnection = this.NewConnection();
            if (IsFirstTime)
            {
                IsFirstTime = false;
                await RaceCondition.ExecuteAsync(async () =>
                {
                    try
                    {
                        await this.TelemetryDefault.CreateIfNotExistsAsync(sqlConnection).NoContext();
                        await this.Dependency.CreateIfNotExistsAsync(sqlConnection).NoContext();
                        await this.Exception.CreateIfNotExistsAsync(sqlConnection).NoContext();
                        await this.Metric.CreateIfNotExistsAsync(sqlConnection).NoContext();
                        await this.Request.CreateIfNotExistsAsync(sqlConnection).NoContext();
                        await this.Trace.CreateIfNotExistsAsync(sqlConnection).NoContext();
                    }
                    catch (Exception ex)
                    {
                        string olaf = ex.ToString();
                    }
                }).NoContext();
            }
            await this.TelemetryDefault.InsertBulkAsync(sqlConnection, Defaults, this.GetProperties(typeof(Telemetry))).NoContext();
            List<Task> eventTasks = new List<Task>();
            foreach (var telemetryObject in Others)
            {
                switch (telemetryObject.Key.Name)
                {
                    case nameof(RequestTelemetry):
                        eventTasks.Add(this.Request.InsertBulkAsync(sqlConnection, telemetryObject.Value, this.GetProperties(typeof(RequestTelemetry))));
                        break;
                    case nameof(TraceTelemetry):
                        eventTasks.Add(this.Trace.InsertBulkAsync(sqlConnection, telemetryObject.Value, this.GetProperties(typeof(TraceTelemetry))));
                        break;
                    case nameof(MetricTelemetry):
                        eventTasks.Add(this.Metric.InsertBulkAsync(sqlConnection, telemetryObject.Value, this.GetProperties(typeof(MetricTelemetry))));
                        break;
                    case nameof(DependencyTelemetry):
                        eventTasks.Add(this.Dependency.InsertBulkAsync(sqlConnection, telemetryObject.Value, this.GetProperties(typeof(DependencyTelemetry))));
                        break;
                    case nameof(ExceptionTelemetry):
                        eventTasks.Add(this.Exception.InsertBulkAsync(sqlConnection, telemetryObject.Value, this.GetProperties(typeof(ExceptionTelemetry))));
                        break;
                    default:
                        SqlTable specialTable = this.GetSpecial(telemetryObject.Key);
                        await specialTable.CreateIfNotExistsAsync(sqlConnection).NoContext();
                        eventTasks.Add(specialTable.InsertBulkAsync(sqlConnection, telemetryObject.Value, this.GetProperties(telemetryObject.Key)));
                        break;
                }
            }
            await Task.WhenAll(eventTasks).NoContext();
        }
        private class MyPropertyInfo : PropertyInfo
        {
            public override PropertyAttributes Attributes => throw new NotImplementedException();
            public MyPropertyInfo(string name, Type propertyType)
            {
                this.Name = name;
                this.PropertyType = propertyType;
            }
            public override bool CanRead => throw new NotImplementedException();

            public override bool CanWrite => throw new NotImplementedException();

            public override Type PropertyType { get; }

            public override Type DeclaringType => throw new NotImplementedException();

            public override string Name { get; }

            public override Type ReflectedType => throw new NotImplementedException();

            public override MethodInfo[] GetAccessors(bool nonPublic)
            {
                throw new NotImplementedException();
            }

            public override object[] GetCustomAttributes(bool inherit)
            {
                throw new NotImplementedException();
            }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                throw new NotImplementedException();
            }

            public override MethodInfo GetGetMethod(bool nonPublic)
            {
                throw new NotImplementedException();
            }

            public override ParameterInfo[] GetIndexParameters()
            {
                throw new NotImplementedException();
            }

            public override MethodInfo GetSetMethod(bool nonPublic)
            {
                throw new NotImplementedException();
            }

            public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
            {
                throw new NotImplementedException();
            }

            public override bool IsDefined(Type attributeType, bool inherit)
            {
                throw new NotImplementedException();
            }

            public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
        private IEnumerable<PropertyInfo> GetProperties(Type type)
        {
            var exactProperties = type.FetchProperties(JsonIgnore);
            List<PropertyInfo> properties = new List<PropertyInfo>();
            if (!exactProperties.Any(x => x.Name == LabelId))
                properties.Add(new MyPropertyInfo(LabelId, typeof(string)));
            if (!exactProperties.Any(x => x.Name == LabelForeignKey))
                properties.Add(new MyPropertyInfo(LabelForeignKey, typeof(string)));
            properties.AddRange(exactProperties);
            return properties;
        }
        private static readonly Type JsonIgnore = typeof(JsonIgnoreAttribute);
        private bool IsSqlPrimitive(Type type)
        {
            if (Primitive.Is(type))
                return true;
            if (type == typeof(DateTime))
                return true;
            if (type == typeof(DateTime?))
                return true;
            return false;
        }
        private Dictionary<string, object> GetValues(ITelemetryEvent telemetryEvent, string telemetryId, PropertyInfo[] propertyInfos)
        {
            Dictionary<string, object> value = new Dictionary<string, object>();
            if (!propertyInfos.Any(x => x.Name == LabelId))
                value.Add(LabelId, Alea.GetTimedKey());
            if (!propertyInfos.Any(x => x.Name == LabelForeignKey))
                value.Add(LabelForeignKey, telemetryId);
            //if (telemetryEvent is IRystemTelemetryEvent)
            //{
            foreach (var property in propertyInfos)
                value.Add(property.Name, this.IsSqlPrimitive(property.PropertyType) ? property.GetValue(telemetryEvent) : property.GetValue(telemetryEvent).ToDefaultJson());
            //}
            //else
            //{
            //    value.Add("Timestamp", telemetryEvent.Timestamp);
            //    value.Add("Json", telemetryEvent.ToDefaultJson());
            //}
            return value;
        }
    }
}