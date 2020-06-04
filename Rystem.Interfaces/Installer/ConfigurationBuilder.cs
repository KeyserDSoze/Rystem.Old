using Rystem.Aggregation;
using Rystem.Azure.Data;
using Rystem.Azure.NoSql;
using Rystem.Azure.Queue;
using Rystem.Cache;
using Rystem.Crypting;
using System;
using System.Collections.Generic;

namespace Rystem
{
    /// <summary>
    /// Allows to start the configuration building through the WithInstallation method
    /// </summary>
    public class ConfigurationBuilder
    {
        internal Dictionary<InstallerType, Dictionary<Installation, IConfiguration>> Configurations { get; } = new Dictionary<InstallerType, Dictionary<Installation, IConfiguration>>();
        internal void AddConfiguration(IConfiguration configuration, InstallerType installerType, Installation installation)
        {
            if (!this.Configurations.ContainsKey(installerType))
                this.Configurations.Add(installerType, new Dictionary<Installation, IConfiguration>());
            this.Configurations[installerType].Add(installation, configuration);
        }
        /// <summary>
        /// Concat a configuration builder to your configuration
        /// </summary>
        /// <returns>Configuration builder</returns>
        public ConfigurationBuilder Concat(ConfigurationBuilder builder)
        {
            foreach (var config in builder.Configurations)
            {
                if (!this.Configurations.ContainsKey(config.Key))
                    this.Configurations.Add(config.Key, new Dictionary<Installation, IConfiguration>());
                foreach (var installer in config.Value)
                {
                    if (!this.Configurations[config.Key].ContainsKey(installer.Key))
                        this.Configurations[config.Key].Add(installer.Key, installer.Value);
                    else
                        throw new ArgumentException($"{installer.Key} already installed.");
                }
            }
            return this;
        }
        /// <summary>
        /// Add a Queue integration
        /// </summary>
        /// <param name="connectionString">ConnectionString for your queue</param>
        /// <returns>specific integration selector</returns>
        public QueueSelector WithQueue(string connectionString)
            => new QueueSelector(connectionString, this);
        /// <summary>
        /// Add a NoSql integration
        /// </summary>
        /// <param name="connectionString">ConnectionString for your sql</param>
        /// <returns>specific integration selector</returns>
        public NoSqlSelector WithNoSql(string connectionString)
            => new NoSqlSelector(connectionString, this);
        /// <summary>
        /// Add a Data integration
        /// </summary>
        /// <param name="connectionString">ConnectionString for your Data</param>
        /// <returns>specific integration selector</returns>
        public DataSelector WithData(string connectionString)
            => new DataSelector(connectionString, this);
        /// <summary>
        /// Add a Crypting integration
        /// </summary>
        /// <returns>specific integration selector</returns>
        public CryptoSelector WithCrypting()
            => new CryptoSelector(this);
        /// <summary>
        /// Add an Aggregation integration
        /// </summary>
        /// <returns>specific integration selector</returns>
        public AggregationSelector<T> WithAggregation<T>()
            => new AggregationSelector<T>(this);
        /// <summary>
        /// Add a Cache integration
        /// </summary>
        /// <returns>specific integration selector</returns>
        public CacheSelector WithCache(CacheConsistency cacheConsistency = CacheConsistency.Always)
            => new CacheSelector(this, cacheConsistency);
        public Dictionary<Installation, IConfiguration> GetConfigurations(InstallerType installerType)
            => this.Configurations[installerType];
    }
}
