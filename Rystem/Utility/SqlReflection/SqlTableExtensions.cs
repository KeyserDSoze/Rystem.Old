using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Utility.SqlReflection
{
    internal static class SqlTableExtensions
    {
        public static DataTable ToDataTable(this IEnumerable<Dictionary<string, object>> data)
        {
            DataTable table = new DataTable();
            foreach (var prop in data.First())
                table.Columns.Add(prop.Key, prop.Value.GetType());
            foreach (var item in data)
            {
                DataRow row = table.NewRow();
                foreach (var prop in item)
                    row[prop.Key] = prop.Value ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }
    }
}