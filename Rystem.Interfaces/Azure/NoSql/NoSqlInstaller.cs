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
        public static void Configure<Entity>(NoSqlConfiguration configuration)
            where Entity : INoSql
        {
            Installer<NoSqlConfiguration, Entity>.Configure(configuration, Installation.Default);
        }
        public static NoSqlConfiguration GetConfiguration<Entity>()
            where Entity : INoSql
        {
            return Installer<NoSqlConfiguration, Entity>.GetConfiguration(Installation.Default);
        }
    }
    public enum NoSqlType
    {
        TableStorage
    }
}
