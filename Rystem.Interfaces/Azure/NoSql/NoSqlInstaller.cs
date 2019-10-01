using Rystem.Enums;
using Rystem.Interfaces;
using System;
using System.Collections.Generic;

namespace Rystem.Azure.NoSql
{
    public class NoSqlConfiguration : IRystemConfiguration
    {
        public string ConnectionString { get; set; }
        public string Name { get; set; }
        public NoSqlType Type { get; set; }
    }
    public static class NoSqlInstaller
    {
        public static void ConfigureAsDefault(NoSqlConfiguration configuration)
        {
            Installer<NoSqlConfiguration>.ConfigureAsDefault(configuration);
        }
        public static void Configure<Entity>(NoSqlConfiguration configuration, Installation installation = Installation.Default)
            where Entity : INoSql
        {
            Installer<NoSqlConfiguration, Entity>.Configure(configuration, installation);
        }
        public static IDictionary<Installation, NoSqlConfiguration> GetConfiguration<Entity>()
            where Entity : INoSql
        {
            return Installer<NoSqlConfiguration, Entity>.GetConfiguration();
        }
    }
    public enum NoSqlType
    {
        TableStorage
    }
}
