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
    internal static class SqlTableExtensions
    {
        public static DataTable ToDataTable(this IEnumerable<Dictionary<string, object>> data, IEnumerable<PropertyInfo> properties, List<string> columns)
        {
            DataTable table = new DataTable();
            foreach (var prop in columns)
                table.Columns.Add(prop, properties.FirstOrDefault(x => x.Name == prop).PropertyType);
            foreach (var item in data)
            {
                DataRow row = table.NewRow();
                foreach (var prop in columns)
                {
                    var value = item.FirstOrDefault(x => x.Key == prop).Value;
                    row[prop] = value != null ? value : DBNull.Value;
                }
                table.Rows.Add(row);
            }
            return table;
        }
    }
}