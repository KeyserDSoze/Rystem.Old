using Rystem.Conversion;
using Rystem.Utility;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace System
{
    public class CsvDefaultConversion<TEntity>
        where TEntity : new()
    {
        private readonly char SplittingChar;
        private const string InCaseOfSplittedChar = "\"{0}\"";
        private const string InNormalCase = "{0}";
        private static readonly string BreakLine = '\n'.ToString();
        private readonly Regex SplittingRegex;
        private static readonly Type CsvIgnoreType = typeof(CsvIgnore);
        private static readonly Type CsvPropertyType = typeof(CsvProperty);
        private static readonly Dictionary<string, PropertyInfo> Properties;
        public CsvDefaultConversion(char splittingChar = ',')
        {
            this.SplittingChar = splittingChar;
            this.SplittingRegex = new Regex($"(\\{this.SplittingChar}|\\r?\\n|\\r|^)(?:\"([^\"]*(?:\"\"[^\"] *) *)\"|([^\"\\{this.SplittingChar}\\r\\n]*))");
        }
        static CsvDefaultConversion()
            => Properties = typeof(TEntity).GetProperties().Where(x => x.GetCustomAttribute(CsvIgnoreType) == null && StringablePrimitive.Check(x.PropertyType)).ToDictionary(x => x.GetCustomAttribute(CsvPropertyType) == null ? x.Name : ((CsvProperty)x.GetCustomAttribute(CsvPropertyType)).Name, x => x);

        public string Write(IEnumerable<TEntity> entities)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string key in Properties.Keys)
                stringBuilder.Append($"{key}{this.SplittingChar}");
            stringBuilder.Remove(stringBuilder.Length - 1, 1);
            stringBuilder.AppendLine();
            foreach (TEntity entity in entities)
            {
                foreach (PropertyInfo propertyInfo in Properties.Values)
                {
                    string value = (propertyInfo.GetValue(entity) ?? string.Empty).ToString();
                    if (value.Contains(SplittingChar))
                        value = string.Format(InCaseOfSplittedChar, value.Replace("\"", "\"\""));
                    else
                        value = string.Format(InNormalCase, value);
                    stringBuilder.Append($"{value}{this.SplittingChar}");
                }
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
                stringBuilder.AppendLine();
            }
            stringBuilder.Remove(stringBuilder.Length - 1, 1);
            return stringBuilder.ToString();
        }
        public async Task<IList<TEntity>> ReadAsync(Stream stream)
        {
            IList<TEntity> datas = new List<TEntity>();
            using (StreamReader sr = new StreamReader(stream))
            {
                string[] properties = this.SplittingRegex.Split(await sr.ReadLineAsync().NoContext());
                while (!sr.EndOfStream)
                    datas.Add(FromString(await sr.ReadLineAsync().NoContext(), properties));
            }
            return datas;
        }
        public IList<TEntity> Read(string value)
        {
            IList<TEntity> datas = new List<TEntity>();
            string[] values = value.Split('\n');
            string[] properties = this.SplittingRegex.Split(values[0]);
            foreach (string entry in values.Skip(1))
                datas.Add(FromString(entry, properties));
            return datas;
        }
        public IList<TEntity> Read(IEnumerable<string> values)
        {
            IList<TEntity> datas = new List<TEntity>();
            string[] properties = this.SplittingRegex.Split(values.FirstOrDefault());
            foreach (string entry in values.Skip(1))
                datas.Add(FromString(entry, properties));
            return datas;
        }
        private TEntity FromString(string value, string[] properties)
        {
            TEntity data = new TEntity();
            string[] values = this.SplittingRegex.Split(value);
            int count = 0;
            foreach (string property in properties)
            {
                if (count >= values.Length)
                    break;
                PropertyInfo propertyInfo = Properties[property];
                propertyInfo.SetValue(data, Convert.ChangeType(values[count].Trim(SplittingChar), propertyInfo.PropertyType));
                count++;
            }
            return data;
        }
    }
}
