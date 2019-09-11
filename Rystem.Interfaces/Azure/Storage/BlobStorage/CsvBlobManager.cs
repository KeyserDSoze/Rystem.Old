using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.WindowsAzure.Storage.Blob;
using Rystem.Interfaces.Conversion;
using Rystem.Utility;

namespace Rystem.Azure.Storage
{
    public class CsvBlobManager : IBlobManager
    {
        private char SplittingChar;
        private const string InCaseOfSplittedChar = "\"{0}\"";
        private const string InNormalCase = "{0}";
        private static readonly string BreakLine = '\n'.ToString();
        private readonly Regex SplittingRegex;
        private static Type CsvIgnoreType = typeof(CsvIgnore);
        private static Dictionary<string, List<PropertyInfo>> Properties = new Dictionary<string, List<PropertyInfo>>();
        private static readonly object TrafficLight = new object();
        public CsvBlobManager(char splittingChar = ',')
        {
            this.SplittingChar = splittingChar;
            this.SplittingRegex = new Regex($"(\\{this.SplittingChar}|\\r?\\n|\\r|^)(?:\"([^\"]*(?:\"\"[^\"] *) *)\"|([^\"\\{this.SplittingChar}\\r\\n]*))");
        }
        private static List<PropertyInfo> Property(Type type)
        {
            if (!Properties.ContainsKey(type.FullName))
            {
                lock (TrafficLight)
                {
                    if (!Properties.ContainsKey(type.FullName))
                    {
                        Properties.Add(type.FullName, type.GetProperties().ToList().FindAll(x => x.GetCustomAttribute(CsvIgnoreType) == null && StringablePrimitive.Check(x.PropertyType) && x.Name != "Name"));
                    }
                }
            }
            return Properties[type.FullName];
        }
        private IBlobStorage Deserialize(Type type, string value)
        {
            IBlobStorage blobStorage = (IBlobStorage)Activator.CreateInstance(type);
            string[] splitting = SplittingRegex.Split(value).Where(x => !string.IsNullOrWhiteSpace(x) && x[0] != SplittingChar).ToArray();
            int count = 0;
            foreach (PropertyInfo propertyInfo in Property(type))
            {
                if (count >= splitting.Length)
                    break;
                propertyInfo.SetValue(blobStorage, Convert.ChangeType(splitting[count].Trim(SplittingChar), propertyInfo.PropertyType));
                count++;
            }
            return blobStorage;
        }
        private string Serialize(IBlobStorage blobStorage)
        {
            StringBuilder stringBuilder = new StringBuilder();
            List<PropertyInfo> propertyInfos = Property(blobStorage.GetType());
            int count = 0;
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                string value = (propertyInfo.GetValue(blobStorage) ?? string.Empty).ToString();
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
        public IBlobStorage OnRetrieve(BlobValue blobValue, Type blobStorageType)
        {
            using (StreamReader sr = new StreamReader(blobValue.MemoryStream))
            {
                IBlobStorage blobStorage = Deserialize(blobStorageType, sr.ReadToEnd());
                blobStorage.BlobProperties = blobValue.BlobProperties;
                blobStorage.Name = blobValue.DestinationFileName;
                return blobStorage;
            }
        }
        public BlobValue Value(IBlobStorage blob)
        {
            return new BlobValue()
            {
                BlobProperties = blob.BlobProperties ?? new BlobProperties() { ContentType = "text/csv" },
                DestinationFileName = blob.Name,
                MemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(Serialize(blob)))
            };
        }
    }
}
