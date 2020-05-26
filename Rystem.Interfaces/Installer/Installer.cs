using Rystem.Aggregation;
using Rystem.Azure.Data;
using Rystem.Azure.NoSql;
using Rystem.Azure.Queue;
using Rystem.Crypting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem
{
    /// <summary>
    /// Allows to select the right integration with "With" methods
    /// </summary>
    public class Installer
    {
        internal readonly ConfigurationBuilder Builder;
        private readonly Installation Installation;
        public Installer(ConfigurationBuilder builder, Installation installation = Installation.Default)
        {
            this.Installation = installation;
            this.Builder = builder;
        }

        internal void AddConfiguration(IConfiguration configuration, InstallerType installerType)
        {
            if (!this.Builder.Configurations.ContainsKey(installerType))
                this.Builder.Configurations.Add(installerType, new Dictionary<Installation, IConfiguration>());
            this.Builder.Configurations[installerType].Add(this.Installation, configuration);
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
    }
}
