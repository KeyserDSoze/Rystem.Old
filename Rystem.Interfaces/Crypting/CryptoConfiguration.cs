namespace Rystem.Crypting
{
    public abstract class CryptoConfiguration : IRystemConfiguration
    {
        public string Name { get; set; }
        public abstract CryptoType Type { get; }
    }
}
