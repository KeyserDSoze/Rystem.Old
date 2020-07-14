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
        public SqlTablePrameterType(string name)
            => this.Value = name;
        public static SqlTablePrameterType BigInt { get; } = new SqlTablePrameterType(nameof(BigInt).ToLower());
        public static SqlTablePrameterType Bit { get; } = new SqlTablePrameterType(nameof(Bit).ToLower());
        public static SqlTablePrameterType Date { get; } = new SqlTablePrameterType(nameof(Date).ToLower());
        public static SqlTablePrameterType Datetime { get; } = new SqlTablePrameterType(nameof(Datetime).ToLower());
        public static SqlTablePrameterType Float { get; } = new SqlTablePrameterType(nameof(Float).ToLower());
        public static SqlTablePrameterType Geography { get; } = new SqlTablePrameterType(nameof(Geography).ToLower());
        public static SqlTablePrameterType Geometry { get; } = new SqlTablePrameterType(nameof(Geometry).ToLower());
        public static SqlTablePrameterType HierarchyId { get; } = new SqlTablePrameterType(nameof(HierarchyId).ToLower());
        public static SqlTablePrameterType Image { get; } = new SqlTablePrameterType(nameof(Image).ToLower());
        public static SqlTablePrameterType Int { get; } = new SqlTablePrameterType(nameof(Int).ToLower());
        public static SqlTablePrameterType Money { get; } = new SqlTablePrameterType(nameof(Money).ToLower());
        public static SqlTablePrameterType NText { get; } = new SqlTablePrameterType(nameof(NText).ToLower());
        public static SqlTablePrameterType Text { get; } = new SqlTablePrameterType(nameof(Text).ToLower());
        public static SqlTablePrameterType Real { get; } = new SqlTablePrameterType(nameof(Real).ToLower());
        public static SqlTablePrameterType SmallDatetime { get; } = new SqlTablePrameterType(nameof(SmallDatetime).ToLower());
        public static SqlTablePrameterType SmallInt { get; } = new SqlTablePrameterType(nameof(SmallInt).ToLower());
        public static SqlTablePrameterType SmallMoney { get; } = new SqlTablePrameterType(nameof(SmallMoney).ToLower());
        public static SqlTablePrameterType SqlVariant { get; } = new SqlTablePrameterType(nameof(SqlVariant).ToLower());
        public static SqlTablePrameterType Timestamp { get; } = new SqlTablePrameterType(nameof(Timestamp).ToLower());
        public static SqlTablePrameterType TinyInt { get; } = new SqlTablePrameterType(nameof(TinyInt).ToLower());
        public static SqlTablePrameterType UniqueIdentifier { get; } = new SqlTablePrameterType(nameof(UniqueIdentifier).ToLower());
        public static SqlTablePrameterType Xml { get; } = new SqlTablePrameterType(nameof(Xml).ToLower());
        public static SqlTablePrameterType Binary(int value = 50) => new SqlTablePrameterType($"{nameof(Binary).ToLower()}({value})");
        public static SqlTablePrameterType Char(int value = 10) => new SqlTablePrameterType($"{nameof(Char).ToLower()}({value})");
        public static SqlTablePrameterType Datetime2(int value = 7) => new SqlTablePrameterType($"{nameof(Datetime2).ToLower()}({value})");
        public static SqlTablePrameterType Time(int value = 7) => new SqlTablePrameterType($"{nameof(Time).ToLower()}({value})");
        public static SqlTablePrameterType DatetimeOffset(int value = 7) => new SqlTablePrameterType($"{nameof(DatetimeOffset).ToLower()}({value})");
        public static SqlTablePrameterType NChar(int value = 10) => new SqlTablePrameterType($"{nameof(NChar).ToLower()}({value})");
        public static SqlTablePrameterType NVarChar(int value = 50) => new SqlTablePrameterType($"{nameof(NVarChar).ToLower()}({value})");
        public static SqlTablePrameterType VarBinary(int value = 50) => new SqlTablePrameterType($"{nameof(VarBinary).ToLower()}({value})");
        public static SqlTablePrameterType VarChar(int value = 100) => new SqlTablePrameterType($"{nameof(VarChar).ToLower()}({value})");
        public static SqlTablePrameterType Decimal(int max = 18, int maxDecimal = 0) => new SqlTablePrameterType($"{nameof(Decimal).ToLower()}({max},{maxDecimal})");
        public static SqlTablePrameterType Numeric(int max = 18, int maxDecimal = 0) => new SqlTablePrameterType($"{nameof(Numeric).ToLower()}({max},{maxDecimal})");
        public static SqlTablePrameterType ByType(Type type)
        {
            if (type == typeof(int))
                return Int;
            if (type == typeof(bool))
                return Bit;
            if (type == typeof(char))
                return Text;
            if (type == typeof(decimal))
                return Decimal();
            if (type == typeof(double))
                return Numeric();
            if (type == typeof(long))
                return BigInt;
            if (type == typeof(byte))
                return Int;
            if (type == typeof(sbyte))
                return SmallInt;
            if (type == typeof(float))
                return Float;
            if (type == typeof(uint))
                return Int;
            if (type == typeof(ulong))
                return BigInt;
            if (type == typeof(short))
                return Int;
            if (type == typeof(ushort))
                return Int;
            if (type == typeof(string))
                return Text;
            if (type == typeof(DateTime))
                return Datetime;
            if (type == typeof(TimeSpan))
                return BigInt;

            if (type == typeof(int?))
                return Int;
            if (type == typeof(bool?))
                return Bit;
            if (type == typeof(char?))
                return Text;
            if (type == typeof(decimal?))
                return Decimal();
            if (type == typeof(double?))
                return Numeric();
            if (type == typeof(long?))
                return BigInt;
            if (type == typeof(byte?))
                return Int;
            if (type == typeof(sbyte?))
                return SmallInt;
            if (type == typeof(float?))
                return Float;
            if (type == typeof(uint?))
                return Int;
            if (type == typeof(ulong?))
                return BigInt;
            if (type == typeof(short?))
                return Int;
            if (type == typeof(ushort?))
                return Int;
            if (type == typeof(DateTime?))
                return Datetime;
            if (type == typeof(TimeSpan?))
                return BigInt;

            if (type == typeof(Guid))
                return Char(36);
            if (type == typeof(Guid?))
                return Char(36);

            return Text;
        }
    }
}
