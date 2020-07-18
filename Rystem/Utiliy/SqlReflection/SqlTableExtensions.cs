using Rystem.Utility.SqlReflection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Utility.SqlReflection
{
    public static class SqlTableExtensions
    {
        public static async Task InsertBulkAsync(this SqlTable sqlTable, SqlConnection connection, IEnumerable<object> values)
        {
            if (values.Any())
            {
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync().NoContext();
                try
                {
                    Func<Task> action = (async () =>
                    {
                        using SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(connection);
                        sqlBulkCopy.DestinationTableName = sqlTable.Name;
                        await sqlBulkCopy.WriteToServerAsync(ToDataTable(values));
                    });
                    await action.Retry(3)
                        .WithCircuitBreak(3, TimeSpan.FromMinutes(5), nameof(SqlBulkCopy))
                        .CatchError(async x =>
                        {
                            if (connection.State != ConnectionState.Open)
                                await connection.OpenAsync().NoContext();
                        })
                        .ExecuteAsync().NoContext();
                }
                catch (Exception ex)
                {
                    string olaf = ex.ToString();
                }
            }

            DataTable ToDataTable(IEnumerable<object> data)
            {
                DataTable table = new DataTable();
                foreach (var prop in sqlTable.Columns)
                    table.Columns.Add(prop.Name, prop.Type.Primitive);
                foreach (var item in data)
                {
                    DataRow row = table.NewRow();
                    foreach (var prop in sqlTable.SetValues(item))
                        row[prop.Key] = prop.Value ?? DBNull.Value;
                    table.Rows.Add(row);
                }
                return table;
            }
        }
    }
}
