using Rystem.Conversion;
using Rystem.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Rystem.Azure.Data.Integration
{
    internal class CsvDataManager<TEntity> : IDataReader<TEntity>, IDataWriter<TEntity>
          where TEntity : IData
    {
        private readonly char SplittingChar;
        private const string InCaseOfSplittedChar = "\"{0}\"";
        private const string InNormalCase = "{0}";
        private static readonly string BreakLine = '\n'.ToString();
        private readonly Regex SplittingRegex;
        private static readonly Type CsvIgnoreType = typeof(CsvIgnore);
        private static readonly Dictionary<string, List<PropertyInfo>> Properties = new Dictionary<string, List<PropertyInfo>>();
        private static readonly object TrafficLight = new object();
        public CsvDataManager(char splittingChar = ',')
        {
            this.SplittingChar = splittingChar;
            this.SplittingRegex = new Regex($"(\\{this.SplittingChar}|\\r?\\n|\\r|^)(?:\"([^\"]*(?:\"\"[^\"] *) *)\"|([^\"\\{this.SplittingChar}\\r\\n]*))");
        }
        private static List<PropertyInfo> Property(Type type)
        {
            if (!Properties.ContainsKey(type.FullName))
                lock (TrafficLight)
                    if (!Properties.ContainsKey(type.FullName))
                        Properties.Add(type.FullName, type.GetProperties().ToList().FindAll(x => x.GetCustomAttribute(CsvIgnoreType) == null && StringablePrimitive.Check(x.PropertyType) && x.Name != "Name"));
            return Properties[type.FullName];
        }
        private TEntity Deserialize(string value)
        {
            TEntity dataLake = (TEntity)Activator.CreateInstance(typeof(TEntity));
            string[] splitting = SplittingRegex.Split(value).Where(x => !string.IsNullOrWhiteSpace(x) && x[0] != SplittingChar).ToArray();
            int count = 0;
            foreach (PropertyInfo propertyInfo in Property(dataLake.GetType()))
            {
                if (count >= splitting.Length)
                    break;
                propertyInfo.SetValue(dataLake, Convert.ChangeType(splitting[count].Trim(SplittingChar), propertyInfo.PropertyType));
                count++;
            }
            return dataLake;
        }
        private string Serialize(IData dataLake)
        {
            StringBuilder stringBuilder = new StringBuilder();
            List<PropertyInfo> propertyInfos = Property(dataLake.GetType());
            int count = 0;
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                string value = (propertyInfo.GetValue(dataLake) ?? string.Empty).ToString();
                if (value.Contains(SplittingChar))
                    value = string.Format(InCaseOfSplittedChar, value.Replace("\"", "\"\""));
                else
                    value = string.Format(InNormalCase, value);
                stringBuilder.Append(value);
                if (count < propertyInfos.Count - 1)
                    stringBuilder.Append(SplittingChar);
                count++;
            }
            return stringBuilder.ToString() + BreakLine;
        }
        public async Task<WrapperEntity<TEntity>> ReadAsync(DataWrapper dummy)
        {
            List<TEntity> aggregatedDatas = new List<TEntity>();
            using (StreamReader sr = new StreamReader(dummy.Stream))
            {
                while (!sr.EndOfStream)
                {
                    TEntity aggregatedData = Deserialize(await sr.ReadLineAsync().NoContext());
                    aggregatedData.Properties = dummy.Properties;
                    aggregatedData.Name = dummy.Name;
                    aggregatedDatas.Add(aggregatedData);
                }
            }
            return new WrapperEntity<TEntity>() { Entities = aggregatedDatas };
        }

        public async Task<DataWrapper> WriteAsync(TEntity entity)
        {
            await Task.Delay(0).NoContext();
            return new DataWrapper()
            {
                Properties = entity.Properties as BlobDataProperties ?? new BlobDataProperties() { ContentType = "text/csv" },
                Name = entity.Name,
                Stream = new MemoryStream(Encoding.UTF8.GetBytes(Serialize(entity)))
            };
        }
    }
}
