using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.AggregatedData
{
    public class AggregatedDataConfiguration<Entity> : IRystemConfiguration
        where Entity : IAggregatedData
    {
        public string ConnectionString { get; set; }
        public string Name { get; set; }
        public AggregatedDataType Type { get; set; }
        public IAggregatedDataReader<Entity> Reader { get; set; }
        public IAggregatedDataListReader<Entity> ListReader { get; set; }
        public IAggregatedDataWriter<Entity> Writer { get; set; }
    }
    public static class AggregatedDataInstaller
    {
        public static void Configure<TEntity>(AggregatedDataConfiguration<TEntity> configuration)
            where TEntity : IAggregatedData
        {
            Installer<AggregatedDataConfiguration<TEntity>, TEntity>.Configure(configuration, Installation.Default);
        }
        public static AggregatedDataConfiguration<TEntity> GetConfiguration<TEntity>()
            where TEntity : IAggregatedData
        {
            return Installer<AggregatedDataConfiguration<TEntity>, TEntity>.GetConfiguration(Installation.Default);
        }
    }
    public enum AggregatedDataType
    {
        BlockBlob,
        AppendBlob,
        PageBlob,
        DataLakeV2
    }
}
