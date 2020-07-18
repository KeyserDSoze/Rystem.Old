using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.Utility.SqlReflection
{
    public class SqlTablePrameterType
    {
        public string Value { get; }
        public Type Primitive { get; }
        public SqlTablePrameterType(string name, Type primitive)
        {
            this.Value = name;
            this.Primitive = primitive;
        }

        public static SqlTablePrameterType BigInt { get; } = new SqlTablePrameterType(nameof(BigInt).ToLower(), typeof(long));
        public static SqlTablePrameterType Bit { get; } = new SqlTablePrameterType(nameof(Bit).ToLower(), typeof(bool));
        public static SqlTablePrameterType Date { get; } = new SqlTablePrameterType(nameof(Date).ToLower(), typeof(DateTime));
        public static SqlTablePrameterType Datetime { get; } = new SqlTablePrameterType(nameof(Datetime).ToLower(), typeof(DateTime));
        public static SqlTablePrameterType Float { get; } = new SqlTablePrameterType(nameof(Float).ToLower(), typeof(float));
        public static SqlTablePrameterType Geography { get; } = new SqlTablePrameterType(nameof(Geography).ToLower(), typeof(object));
        public static SqlTablePrameterType Geometry { get; } = new SqlTablePrameterType(nameof(Geometry).ToLower(), typeof(object));
        public static SqlTablePrameterType HierarchyId { get; } = new SqlTablePrameterType(nameof(HierarchyId).ToLower(), typeof(object));
        public static SqlTablePrameterType Image { get; } = new SqlTablePrameterType(nameof(Image).ToLower(), typeof(byte[]));
        public static SqlTablePrameterType Int { get; } = new SqlTablePrameterType(nameof(Int).ToLower(), typeof(int));
        public static SqlTablePrameterType Money { get; } = new SqlTablePrameterType(nameof(Money).ToLower(), typeof(decimal));
        public static SqlTablePrameterType NText { get; } = new SqlTablePrameterType(nameof(NText).ToLower(), typeof(string));
        public static SqlTablePrameterType Text { get; } = new SqlTablePrameterType(nameof(Text).ToLower(), typeof(string));
        public static SqlTablePrameterType Real { get; } = new SqlTablePrameterType(nameof(Real).ToLower(), typeof(double));
        public static SqlTablePrameterType SmallDatetime { get; } = new SqlTablePrameterType(nameof(SmallDatetime).ToLower(), typeof(DateTime));
        public static SqlTablePrameterType SmallInt { get; } = new SqlTablePrameterType(nameof(SmallInt).ToLower(), typeof(Int16));
        public static SqlTablePrameterType SmallMoney { get; } = new SqlTablePrameterType(nameof(SmallMoney).ToLower(), typeof(decimal));
        public static SqlTablePrameterType SqlVariant { get; } = new SqlTablePrameterType(nameof(SqlVariant).ToLower(), typeof(object));
        public static SqlTablePrameterType Timestamp { get; } = new SqlTablePrameterType(nameof(Timestamp).ToLower(), typeof(DateTime));
        public static SqlTablePrameterType TinyInt { get; } = new SqlTablePrameterType(nameof(TinyInt).ToLower(), typeof(Int16));
        public static SqlTablePrameterType UniqueIdentifier { get; } = new SqlTablePrameterType(nameof(UniqueIdentifier).ToLower(), typeof(Guid));
        public static SqlTablePrameterType Xml { get; } = new SqlTablePrameterType(nameof(Xml).ToLower(), typeof(string));
        public static SqlTablePrameterType Binary(int value = 50) => new SqlTablePrameterType($"{nameof(Binary).ToLower()}({value})", typeof(byte[]));
        public static SqlTablePrameterType Char(int value = 10) => new SqlTablePrameterType($"{nameof(Char).ToLower()}({value})", typeof(string));
        public static SqlTablePrameterType Datetime2(int value = 7) => new SqlTablePrameterType($"{nameof(Datetime2).ToLower()}({value})", typeof(DateTime));
        public static SqlTablePrameterType Time(int value = 7) => new SqlTablePrameterType($"{nameof(Time).ToLower()}({value})", typeof(TimeSpan));
        public static SqlTablePrameterType DatetimeOffset(int value = 7) => new SqlTablePrameterType($"{nameof(DatetimeOffset).ToLower()}({value})", typeof(TimeSpan));
        public static SqlTablePrameterType NChar(int value = 10) => new SqlTablePrameterType($"{nameof(NChar).ToLower()}({value})", typeof(string));
        public static SqlTablePrameterType NVarChar(int value = 50) => new SqlTablePrameterType($"{nameof(NVarChar).ToLower()}({value})", typeof(string));
        public static SqlTablePrameterType VarBinary(int value = 50) => new SqlTablePrameterType($"{nameof(VarBinary).ToLower()}({value})", typeof(byte[]));
        public static SqlTablePrameterType VarChar(int value = 100) => new SqlTablePrameterType($"{nameof(VarChar).ToLower()}({value})", typeof(string));
        public static SqlTablePrameterType Decimal(int max = 18, int maxDecimal = 0) => new SqlTablePrameterType($"{nameof(Decimal).ToLower()}({max},{maxDecimal})", typeof(decimal));
        public static SqlTablePrameterType Numeric(int max = 18, int maxDecimal = 0) => new SqlTablePrameterType($"{nameof(Numeric).ToLower()}({max},{maxDecimal})", typeof(decimal));
    }
}