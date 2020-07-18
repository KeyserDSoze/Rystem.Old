using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Utility.SqlReflection
{
    public class SqlColumn
    {
        public string Name { get; }
        public SqlTablePrameterType Type { get; }
        public bool IsIdentity { get; private set; }
        public bool IsPrimaryKey { get; private set; }
        public bool IsNotNullable { get; private set; }
        public string Foreign { get; private set; }
        public SqlTable Table { get; }
        public SqlColumn(string name, SqlTablePrameterType type, SqlTable table)
        {
            this.Name = name;
            this.Type = type;
            this.Table = table;
        }
        public SqlColumn AsNotNullable()
        {
            this.IsNotNullable = true;
            return this;
        }
        public SqlColumn AsIdentity()
        {
            this.IsIdentity = true;
            this.IsPrimaryKey = true;
            return this;
        }
        public SqlColumn AsPrimaryKey()
        {
            this.IsPrimaryKey = true;
            return this;
        }
        public SqlColumn WithForeignKey(string tableName, string columnName)
        {
            this.Foreign = $"{tableName}({columnName})";
            return this;
        }
        public Func<object, object> GetValue;
        public SqlTable Compose<TEntity>(Func<TEntity, object> getValue)
        {
            this.GetValue = x => getValue((TEntity)x);
            return this.Table;
        }

        private const string NotNull = "NOT NULL";
        private const string Identity = "IDENTITY(1,1)";
        private const string PrimaryKey = "PRIMARY KEY";
        private const string ForeignKey = "FOREIGN KEY REFERENCES";
        public override string ToString()
            => $"[{Name}] {Type.Value} {(IsNotNullable ? NotNull : string.Empty)} {(IsIdentity ? Identity : string.Empty)} {(IsPrimaryKey ? PrimaryKey : string.Empty)} {(Foreign != null ? $"{ForeignKey} {Foreign}" : string.Empty)}";
    }
}
