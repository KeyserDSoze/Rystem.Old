namespace Rystem.DistributedLock
{
    public class LockConfiguration : IConfiguration
    {
        public string ConnectionString { get; set; }
        public string Name { get; set; }
        public LockType Type { get; set; }
        internal LockConfiguration() { }
    }
}