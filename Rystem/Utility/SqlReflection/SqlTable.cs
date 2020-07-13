using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Utility.SqlReflection
{
    public class SqlTable
    {
        public string Name { get; }
        public List<SqlTableParameter> Parameters { get; } = new List<SqlTableParameter>();
        public string ParametersName => string.Join(",", Parameters.Where(x => !x.IsIdentity).Select(x => x.Name));
        public string ParametersValue => string.Join(",", Parameters.Where(x => !x.IsIdentity).Select(x => $"@{x.Name}"));
        public string IdentityName => Parameters.FirstOrDefault(x => x.IsIdentity).Name;
        public string InsertQuery => $"Insert into {Name} ({ParametersName}) VALUES ({ParametersValue});";
        public string InsertQueryWithOutputPrimaryKey => $"Insert into {Name} ({ParametersName}) OUTPUT Inserted.{IdentityName} VALUES ({ParametersValue});";
        public SqlTable(string name)
            => this.Name = name;
        public SqlTable WithNotNullable(string name, SqlTablePrameterType type)
        {
            this.Parameters.Add(SqlTableParameter.CreateNotNullable(name, type));
            return this;
        }
        public SqlTable WithNullable(string name, SqlTablePrameterType type)
        {
            this.Parameters.Add(SqlTableParameter.CreateNullable(name, type));
            return this;
        }
        public SqlTable WithPrimaryKey(string name, SqlTablePrameterType type)
        {
            this.Parameters.Add(SqlTableParameter.CreatePrimaryKey(name, type));
            return this;
        }
        public SqlTable WithForeignKey(string name, SqlTablePrameterType type)
        {
            this.Parameters.Add(SqlTableParameter.CreatePrimaryKey(name, type));
            return this;
        }
        public SqlTable WithPrimaryKeyAndIdentity(string name, SqlTablePrameterType type)
        {
            this.Parameters.Add(SqlTableParameter.CreatePrimaryKeyWithIdentity(name, type));
            return this;
        }
        public async Task CreateIfNotExistsAsync(SqlConnection connection)
        {
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync().NoContext();
            StringBuilder sb = new StringBuilder();
            sb.Append($"IF NOT EXISTS (SELECT TOP 1 * FROM sysobjects WHERE name='{Name}' and xtype='U')");
            sb.Append($"CREATE TABLE {Name} (");
            sb.Append(string.Join(',', Parameters.Select(x => x.ToString())));
            sb.Append(");");
            using SqlCommand command = new SqlCommand(sb.ToString(), connection);
            await command.ExecuteNonQueryAsync().NoContext();
        }
        public async Task InsertBulkAsync(SqlConnection connection, List<Dictionary<string, object>> allParameters)
        {
            if (allParameters.Any())
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    await connection.OpenAsync().NoContext();
                using SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(connection);
                sqlBulkCopy.DestinationTableName = this.Name;
                await sqlBulkCopy.WriteToServerAsync(allParameters.ToDataTable());
            }
        }
    }
}