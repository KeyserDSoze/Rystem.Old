using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Azure.Storage
{
    /// <summary>
    /// Permette l'installazione di tutte le ConnectionString su tutte le entity del progetto.
    /// E' possibile sovrascriverle una ad una chiamando <see cref="BlobStorageInstall{TBlob}"/>.
    /// </summary>
    public static class BlobStorageInstaller
    {
        private static string ConnectionStringDefault;
        private static string ContainerDefault;
        public static BlobStorageType BlobStorageTypeDefault;
        private static Dictionary<string, BlobConfiguration> Contexts = new Dictionary<string, BlobConfiguration>();
        public class BlobConfiguration
        {
            public string ConnectionString { get; set; }
            public string Container { get; set; }
            public BlobStorageType BlobStorageType { get; set; }
        }
        /// <summary>
        /// Installa la ConnectionString su tutte le Entity del progetto.
        /// </summary>
        /// <param name="connectionString">stringa di connessione del Blob Storage</param>
        /// <example>
        /// <code>
        /// static void OnAppStartup(){
        /// #if DEBUG
        ///     BlobStorageInstaller.Configure(StagingConnectionString);
        /// #else
        ///     BlobStorageInstaller.Configure(ProductionConnectionString);
        /// #endif
        /// }
        /// </code>
        /// </example>
        public static void Configure(string connectionString, BlobStorageType blobStorageType = BlobStorageType.Unspecified, string container = null)
        {
            ConnectionStringDefault = connectionString;
            ContainerDefault = container;
            BlobStorageTypeDefault = blobStorageType;
        }
        public static void Configure<TEntry>(string connectionString, BlobStorageType blobStorageType = BlobStorageType.Unspecified, string container = null)
            where TEntry : IBlob
        {
            Type type = typeof(TEntry);
            if (!Contexts.ContainsKey(type.FullName))
                Contexts.Add(type.FullName, new BlobConfiguration()
                {
                    ConnectionString = connectionString,
                    Container = !string.IsNullOrWhiteSpace(container) ? container : (!string.IsNullOrWhiteSpace(ContainerDefault) ? ContainerDefault : type.Name.ToLower()),
                    BlobStorageType =  blobStorageType != BlobStorageType.Unspecified ? blobStorageType : (BlobStorageTypeDefault != BlobStorageType.Unspecified  ? BlobStorageTypeDefault : BlobStorageType.BlockBlob)
                });
        }
        public static BlobConfiguration GetConnectionString(Type type)
        {
            if (Contexts.ContainsKey(type.FullName))
                return Contexts[type.FullName];
            if (!string.IsNullOrWhiteSpace(ConnectionStringDefault))
                return new BlobConfiguration()
                {
                    ConnectionString = ConnectionStringDefault,
                    Container = !string.IsNullOrWhiteSpace(ContainerDefault) ? ContainerDefault : type.Name.ToLower(),
                    BlobStorageType = BlobStorageTypeDefault != BlobStorageType.Unspecified ? BlobStorageTypeDefault : BlobStorageType.BlockBlob
                };
            throw new NotImplementedException("Please use Install static method in static constructor of your class to set ConnectionString");
        }
    }
    public enum BlobStorageType
    {
        Unspecified = 0,
        PageBlob = 1,
        BlockBlob = 2,
        AppendBlob = 3
    }
}
