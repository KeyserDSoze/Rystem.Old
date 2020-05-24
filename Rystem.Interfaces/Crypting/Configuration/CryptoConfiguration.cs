namespace Rystem.Crypting
{
    public abstract class CryptoConfiguration : IConfiguration
    {
        public abstract CryptoType Type { get; }
    }
}
