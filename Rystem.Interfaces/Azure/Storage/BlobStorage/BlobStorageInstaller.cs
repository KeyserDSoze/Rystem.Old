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
        private static BlobConfiguration Default;
        private static Dictionary<string, BlobConfiguration> Contexts = new Dictionary<string, BlobConfiguration>();
        public class BlobConfiguration
        {
            public string ConnectionString { get; set; }
            public string Container { get; set; }
            public BlobStorageType BlobStorageType { get; set; }
            public IBlobManager BlobManager { get; set; }
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
        public static void ConfigureAsDefault(string connectionString, BlobStorageType blobStorageType = BlobStorageType.Unspecified, string container = null, IBlobManager blobManager = default(JsonBlobManager))
        {
            Default = new BlobConfiguration()
            {
                ConnectionString = connectionString,
                Container = container,
                BlobStorageType = blobStorageType,
                BlobManager = blobManager ?? new JsonBlobManager()
            };
        }
        public static void Configure<TEntry>(string connectionString, BlobStorageType blobStorageType = BlobStorageType.Unspecified, string container = null, IBlobManager blobManager = default(JsonBlobManager))
            where TEntry : IBlobStorage
        {
            Type type = typeof(TEntry);
            if (!Contexts.ContainsKey(type.FullName))
                Contexts.Add(type.FullName, new BlobConfiguration()
                {
                    ConnectionString = connectionString,
                    Container = !string.IsNullOrWhiteSpace(container) ? container :
                        (Default != null && !string.IsNullOrWhiteSpace(Default.Container) ? Default.Container
                            : type.Name.ToLower()),
                    BlobStorageType = blobStorageType != BlobStorageType.Unspecified ? blobStorageType :
                        (Default != null && Default.BlobStorageType != BlobStorageType.Unspecified ? Default.BlobStorageType
                            : BlobStorageType.BlockBlob),
                    BlobManager = blobManager ?? Default?.BlobManager ?? new JsonBlobManager()
                });
        }
        public static BlobConfiguration GetConnectionString(Type type)
        {
            if (Contexts.ContainsKey(type.FullName))
                return Contexts[type.FullName];
            if (Default != null && !string.IsNullOrWhiteSpace(Default.ConnectionString))
                return Default;
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
