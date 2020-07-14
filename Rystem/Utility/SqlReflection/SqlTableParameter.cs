using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Utility.SqlReflection
{
    public class SqlTableParameter
    {
        public string Name { get; }
        public SqlTablePrameterType Type { get; }
        public bool IsIdentity { get; }
        public bool IsPrimaryKey { get; }
        public bool IsNullable { get; }
        public string Foreign { get; }
        private SqlTableParameter(string name, SqlTablePrameterType tablePrameterType, bool isIdentity, bool isPrimaryKey, bool isNullable, string foreign)
        {
            this.Name = name;
            this.Type = tablePrameterType;
            this.IsIdentity = isIdentity;
            this.IsNullable = isNullable;
            this.IsPrimaryKey = isPrimaryKey;
            this.Foreign = foreign;
        }
        public static SqlTableParameter CreateNotNullable(string name, SqlTablePrameterType type, string foreign = null)
            => Create(name, type, false, false, false, foreign);
        public static SqlTableParameter CreateNullable(string name, SqlTablePrameterType type, string foreign = null)
            => Create(name, type, false, false, true, foreign);
        public static SqlTableParameter CreatePrimaryKey(string name, SqlTablePrameterType type)
            => Create(name, type, false, true, false, null);
        public static SqlTableParameter CreatePrimaryKeyWithIdentity(string name, SqlTablePrameterType type, string foreign = null)
            => Create(name, type, true, true, false, foreign);
        public static SqlTableParameter Create(string name, SqlTablePrameterType type, bool isIdentity, bool isPrimaryKey, bool isNullable, string foreign)
            => new SqlTableParameter(name, type, isIdentity, isPrimaryKey, isNullable, foreign);
        private const string NotNull = "NOT NULL";
        private const string Identity = "IDENTITY(1,1)";
        private const string PrimaryKey = "PRIMARY KEY";
        private const string ForeignKey = "FOREIGN KEY REFERENCES";
        public override string ToString()
            => $"[{Name}] {Type.Value} {(IsNullable ? string.Empty : NotNull)} {(IsIdentity ? Identity : string.Empty)} {(IsPrimaryKey ? PrimaryKey : string.Empty)} {(Foreign != null ? $"{ForeignKey} {Foreign}" : string.Empty)}";
    }
}
