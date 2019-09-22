using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.DataLake
{
    public class DataLakeConfiguration : IRystemConfiguration
    {
        public string ConnectionString { get; set; }
        public string Name { get; set; }
        public DataLakeType Type { get; set; }
        public IDataLakeReader Reader { get; set; }
        public IDataLakeWriter Writer { get; set; }
    }
    public static class DataLakeInstaller
    {
        public static void ConfigureAsDefault(DataLakeConfiguration configuration)
        {
            Installer<DataLakeConfiguration>.ConfigureAsDefault(configuration);
        }
        public static void Configure<Entity>(DataLakeConfiguration configuration)
            where Entity : IDataLake
        {
            Installer<DataLakeConfiguration, Entity>.Configure(configuration, Installation.Default);
        }
        public static DataLakeConfiguration GetConfiguration<Entity>()
            where Entity : IDataLake
        {
            return Installer<DataLakeConfiguration, Entity>.GetConfiguration(Installation.Default);
        }
    }
    public enum DataLakeType
    {
        BlockBlob,
        AppendBlob,
        PageBlob,
        DataLakeV2
    }
}
