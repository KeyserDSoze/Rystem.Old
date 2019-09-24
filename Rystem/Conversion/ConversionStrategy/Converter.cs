using Rystem.Interfaces.Conversion;
using Rystem.Utility;
using System;
using System.Collections;
using System.Reflection;
using System.Text;

namespace Rystem.Conversion
{
    public abstract class Converter
    {
        private protected IConverterFactory Factory;
        private protected int Index;
        public Converter(IConverterFactory factory, int index) { this.Factory = factory; this.Index = index; }
        public abstract string Convert(object value);
    }
    public class ObjectConverter : Converter
    {
        public ObjectConverter(IConverterFactory factory, int index) : base(factory, index) { }
        private static readonly Type Ignore = typeof(CsvIgnore);
        public override string Convert(object value)
        {
            if (value == null)
                return string.Empty;
            StringBuilder stringBuilder = new StringBuilder();
            foreach (PropertyInfo property in Properties.Fetch(value.GetType(), Ignore))
                stringBuilder.Append($"{this.Factory.GetConverter(property.PropertyType, this.Index).Convert(property.GetValue(value))}{(char)this.Index}");
            return stringBuilder.ToString().Trim((char)this.Index);
        }
    }
    public class EnumerableConverter : Converter
    {
        public EnumerableConverter(IConverterFactory factory, int index) : base(factory, index) { }
        public override string Convert(object values)
        {
            if (values is IDictionary)
                return new DictionaryConverter(this.Factory, this.Index).Convert(values);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (object value in values as IEnumerable)
            {
                stringBuilder.Append($"{this.Factory.GetConverter(value.GetType(), this.Index).Convert(value)}■");
            }
            return stringBuilder.ToString().Trim('■');
        }
    }
    public class DictionaryConverter : Converter
    {
        public DictionaryConverter(IConverterFactory factory, int index) : base(factory, index) { }
        public override string Convert(object values)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (DictionaryEntry entry in values as IDictionary)
            {
                string key = this.Factory.GetConverter(entry.Key.GetType(), this.Index).Convert(entry.Key);
                string value = this.Factory.GetConverter(entry.Value.GetType(), this.Index).Convert(entry.Value);
                stringBuilder.Append($"{key}¶{value}■");
            }
            return stringBuilder.ToString().Trim('■');
        }
    }
#warning Abstract e Interface non predisposte
    public class AbstractInterfaceConverter : Converter
    {
        public AbstractInterfaceConverter(IConverterFactory factory, int index) : base(factory, index)
        {
        }

        public override string Convert(object value)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"{value.GetType().FullName}|");
            builder.Append(this.Factory.GetConverter(value.GetType(), this.Index).Convert(value));
            return builder.ToString();
        }
    }
    public class PrimitiveConverter : Converter
    {
        public PrimitiveConverter(IConverterFactory factory, int index) : base(factory, index) { }
        public override string Convert(object value)
        {
            return value.ToString();
        }
    }
    public interface IConverterFactory
    {
        Converter GetConverter(Type valueType, int index);
    }
    public class ConverterFactory : IConverterFactory
    {
        public Converter GetConverter(Type objectType, int index)
        {
            if (StringablePrimitive.CheckWithNull(objectType))
                return new PrimitiveConverter(this, index + 1);
            else if (typeof(IEnumerable).IsAssignableFrom(objectType))
                return new EnumerableConverter(this, index + 1);
            else
                return new ObjectConverter(this, index + 1);
        }
    }
}
