namespace Rystem.DistributedLock
{
    public class LockSelector : IBuildingSelector
    {
        private readonly string ConnectionString;
        public ConfigurationBuilder Builder { get; }

        internal LockSelector(string connectionString, ConfigurationBuilder builder)
        {
            this.ConnectionString = connectionString;
            this.Builder = builder;
        }
        public LockBuilder WithBlobStorage(BlobBuilder blobBuilder)
        {
            blobBuilder.LockConfiguration.ConnectionString = this.ConnectionString;
            return new LockBuilder(blobBuilder.LockConfiguration, this);
        }
        public LockBuilder WithRedisCache(RedisCacheBuilder redisCacheBuilder)
        {
            redisCacheBuilder.LockConfiguration.ConnectionString = this.ConnectionString;
            return new LockBuilder(redisCacheBuilder.LockConfiguration, this);
        }
    }
}