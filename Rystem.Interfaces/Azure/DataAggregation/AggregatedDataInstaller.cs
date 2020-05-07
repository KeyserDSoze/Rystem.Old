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
        public static void Configure<TEntity>(AggregatedDataConfiguration<TEntity> configuration, Installation installation = Installation.Default)
            where TEntity : IAggregatedData
            => Installer<AggregatedDataConfiguration<TEntity>, TEntity>.Configure(configuration, installation);
        public static IDictionary<Installation, AggregatedDataConfiguration<TEntity>> GetConfiguration<TEntity>()
            where TEntity : IAggregatedData
            => Installer<AggregatedDataConfiguration<TEntity>, TEntity>.GetConfiguration();
    }
    public enum AggregatedDataType
    {
        BlockBlob,
        AppendBlob,
        PageBlob,
        DataLakeV2
    }
}
