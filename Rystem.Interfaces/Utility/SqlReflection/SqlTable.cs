using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Utility.SqlReflection
{
    public class SqlTable
    {
        public string Name { get; }
        public List<SqlColumn> Columns { get; } = new List<SqlColumn>();
        public string ParametersName => string.Join(",", Columns.Where(x => !x.IsIdentity).Select(x => x.Name));
        public string ParametersValue => string.Join(",", Columns.Where(x => !x.IsIdentity).Select(x => $"@{x.Name}"));
        public string IdentityName => Columns.FirstOrDefault(x => x.IsIdentity).Name;
        public string InsertQuery => $"Insert into {Name} ({ParametersName}) VALUES ({ParametersValue});";
        public string InsertQueryWithOutputPrimaryKey => $"Insert into {Name} ({ParametersName}) OUTPUT Inserted.{IdentityName} VALUES ({ParametersValue});";
        public string SelectQuery(string where) => $"select * from {this.Name} {where}";
        public Type InstanceOf { get; private set; }
        public SqlTable(string name)
            => this.Name = name;
        public SqlTable AddType(Type instanceOf)
        {
            this.InstanceOf = instanceOf;
            return this;
        }
        public SqlColumn With(string name, SqlTablePrameterType type)
        {
            var column = new SqlColumn(name, type, this);
            this.Columns.Add(column);
            return column;
        }
        public SqlTable AddColumn(SqlColumn column)
        {
            Columns.Add(column);
            return this;
        }
        public Dictionary<string, object> SetValues(object telemetryEvent)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();
            foreach (var column in this.Columns)
                values.Add(column.Name, column.GetValue(telemetryEvent));
            return values;
        }
        private const string AddColumnAlteringTable = "ALTER TABLE {0} ADD {1};";
        private const string DropColumnAlteringTable = "ALTER TABLE {0} DROP COLUMN {1};";
        public async Task CreateIfNotExistsAsync(SqlConnection connection, bool checkNewParameters = false)
        {
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync().NoContext();
            StringBuilder sb = new StringBuilder();
            sb.Append($"IF NOT EXISTS (SELECT TOP 1 * FROM sysobjects WHERE name='{Name}' and xtype='U')");
            sb.Append($"CREATE TABLE {Name} (");
            sb.Append(string.Join(',', Columns.Select(x => x.ToString())));
            sb.Append(");");
            using SqlCommand command = new SqlCommand(sb.ToString(), connection);
            await command.ExecuteNonQueryAsync().NoContext();
            if (this.NamedColumns == null)
            {
                await CheckColumnAsync().NoContext();
                if (checkNewParameters)
                {
                    List<SqlColumn> newParameters = new List<SqlColumn>();
                    List<string> removeParameters = new List<string>();
                    foreach (var parameter in this.Columns)
                        if (!NamedColumns.Contains(parameter.Name))
                            newParameters.Add(parameter);
                    foreach (var column in NamedColumns)
                        if (!this.Columns.Any(x => x.Name == column))
                            removeParameters.Add(column);
                    StringBuilder alteringQuery = new StringBuilder();
                    if (newParameters.Count > 0)
                        foreach (var newParameter in newParameters)
                            alteringQuery.Append(string.Format(AddColumnAlteringTable, Name, newParameter.ToString()));
                    if (removeParameters.Count > 0)
                        foreach (var removeParameter in removeParameters)
                            alteringQuery.Append(string.Format(DropColumnAlteringTable, Name, removeParameter));
                    if (alteringQuery.Length > 0)
                        await new SqlCommand(alteringQuery.ToString(), connection).ExecuteNonQueryAsync();
                    await CheckColumnAsync().NoContext();
                }
                async Task CheckColumnAsync()
                {
                    this.NamedColumns = new List<string>();
                    using SqlDataReader reader = await new SqlCommand($"select COLUMN_NAME from information_schema.columns where table_name = '{this.Name}'", connection).ExecuteReaderAsync().NoContext();
                    while (await reader.ReadAsync().NoContext())
                        NamedColumns.Add(reader[0].ToString());
                }
            }
        }
        public async IAsyncEnumerable<object> GetAsync(SqlConnection sqlConnection, string where, IEnumerable<SqlParameter> parameters)
        {
            if (sqlConnection.State != ConnectionState.Open)
                await sqlConnection.OpenAsync().NoContext();
            using SqlCommand sqlCommand = new SqlCommand(this.SelectQuery(where), sqlConnection);
            foreach (SqlParameter parameter in parameters)
                sqlCommand.Parameters.Add(parameter);
            using SqlDataReader reader = await sqlCommand.ExecuteReaderAsync().NoContext();
            while (await reader.ReadAsync().NoContext())
            {
                var entity = Activator.CreateInstance(this.InstanceOf);
                foreach (var column in this.Columns)
                    if (column.SetValue != null && !reader.IsDBNull(column.Name))
                        column.SetValue(entity, Convert.ChangeType(reader[column.Name], column.Type.Primitive));
                yield return entity;
            }
        }
        public List<string> NamedColumns { get; private set; }
    }
}