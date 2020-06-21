namespace Rystem.DistributedLock
{
    public class RedisCacheBuilder
    {
        public LockConfiguration LockConfiguration { get; }
        public RedisCacheBuilder(string name = null, string key = null)
        {
            this.LockConfiguration = new LockConfiguration()
            {
                Name = name,
                Key = key,
                Type = LockType.RedisCache
            };
        }
    }
}