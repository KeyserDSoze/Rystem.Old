namespace Rystem.DistributedLock
{
    public class RedisCacheBuilder
    {
        public LockConfiguration LockConfiguration { get; }
        public RedisCacheBuilder(string name)
        {
            this.LockConfiguration = new LockConfiguration()
            {
                Name = name,
                Type = LockType.RedisCache
            };
        }
    }
}