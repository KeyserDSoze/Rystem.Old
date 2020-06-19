namespace Rystem.DistributedLock
{
    public class BlobBuilder
    {
        public LockConfiguration LockConfiguration { get; }
        public BlobBuilder(string name = null)
        {
            this.LockConfiguration = new LockConfiguration()
            {
                Name = name,
                Type = LockType.BlobStorage
            };
        }
    }
}
