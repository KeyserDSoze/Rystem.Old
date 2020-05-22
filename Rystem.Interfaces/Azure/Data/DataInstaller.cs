using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.Data
{
    public class DataConfiguration<Entity> : IRystemConfiguration
        where Entity : IData
    {
        public string ConnectionString { get; set; }
        public string Name { get; set; }
        public AggregatedDataType Type { get; set; }
        public IDataReader<Entity> Reader { get; set; }
        public IDataWriter<Entity> Writer { get; set; }
    }
    public static class DataInstaller
    {
        public static void Configure<TEntity>(DataConfiguration<TEntity> configuration, Installation installation = Installation.Default)
            where TEntity : IData
            => Installer<DataConfiguration<TEntity>, TEntity>.Configure(configuration, installation);
        public static IDictionary<Installation, DataConfiguration<TEntity>> GetConfiguration<TEntity>()
            where TEntity : IData
            => Installer<DataConfiguration<TEntity>, TEntity>.GetConfiguration();
    }
    public enum AggregatedDataType
    {
        BlockBlob,
        AppendBlob,
        PageBlob,
        DataLakeV2
    }
}
