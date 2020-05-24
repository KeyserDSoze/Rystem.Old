using Rystem.Azure.Data;
using Rystem.Azure.NoSql;
using Rystem.Azure.Queue;
using Rystem.Crypting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem
{
    public class Installer
    {
        internal readonly ConfigurationBuilder Builder;
        private readonly Installation Installation;
        public Installer(ConfigurationBuilder azureBuilder, Installation installation = Installation.Default)
        {
            this.Installation = installation;
            this.Builder = azureBuilder;
        }

        internal void AddConfiguration(IConfiguration configuration)
            => this.Builder.Configurations.Add(this.Installation, configuration);
        public QueueSelector WithQueue(string connectionString)
            => new QueueSelector(connectionString, this);
        public NoSqlSelector WithNoSql(string connectionString)
            => new NoSqlSelector(connectionString, this);
        public DataSelector WithData(string connectionString)
            => new DataSelector(connectionString, this);
        public CryptoSelector WithCrypting()
            => new CryptoSelector(this);
    }
}
