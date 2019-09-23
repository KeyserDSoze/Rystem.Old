using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.DataLake
{
    public class DataLakeConfiguration<Entity> : IRystemConfiguration
        where Entity : IDataLake
    {
        public string ConnectionString { get; set; }
        public string Name { get; set; }
        public DataLakeType Type { get; set; }
        public IDataLakeReader<Entity> Reader { get; set; }
        public IDataLakeWriter Writer { get; set; }
    }
    public static class DataLakeInstaller
    {
        public static void Configure<TEntity>(DataLakeConfiguration<TEntity> configuration)
            where TEntity : IDataLake
        {
            Installer<DataLakeConfiguration<TEntity>, TEntity>.Configure(configuration, Installation.Default);
        }
        public static DataLakeConfiguration<TEntity> GetConfiguration<TEntity>()
            where TEntity : IDataLake
        {
            return Installer<DataLakeConfiguration<TEntity>, TEntity>.GetConfiguration(Installation.Default);
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
