namespace Rystem.Crypting
{
    internal interface ICryptoIntegration
    {
        string Encrypt(string message);
        string Decrypt(string encryptedMessage);
    }
}
