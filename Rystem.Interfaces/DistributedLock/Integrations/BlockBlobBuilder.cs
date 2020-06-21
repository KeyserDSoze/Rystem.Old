namespace Rystem.DistributedLock
{
    public class BlobBuilder
    {
        public LockConfiguration LockConfiguration { get; }
        public BlobBuilder(string name = null, string key = null)
        {
            this.LockConfiguration = new LockConfiguration()
            {
                Name = name,
                Key = key,
                Type = LockType.BlobStorage
            };
        }
    }
}
