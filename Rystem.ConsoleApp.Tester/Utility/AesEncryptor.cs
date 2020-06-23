using Rystem.Crypting;
using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.Utility
{
    public class AesEncryptor : IUnitTest
    {
        public async Task DoWorkAsync(Action<object> action, UnitTestMetrics metrics, params string[] args)
        {
            await Task.Delay(0).NoContext();
            AesHelper aesHelper = new AesHelper()
            {
                Message = "a.rapiti@vetrya.com"
            };
            string a = aesHelper.Encrypt().CryptedMessage;
            string b = aesHelper.Encrypt().CryptedMessage;
            metrics.CheckIfNotOkExit(a == b);
            metrics.CheckIfNotOkExit(aesHelper.Decrypt().Message != "a.rapiti@vetrya.com");
            a = aesHelper.Encrypt(Installation.Inst00).CryptedMessage;
            b = aesHelper.Encrypt(Installation.Inst00).CryptedMessage;
            metrics.CheckIfNotOkExit(a != b);
            metrics.CheckIfNotOkExit(aesHelper.Decrypt(Installation.Inst00).Message != "a.rapiti@vetrya.com");
        }
    }
    public class AesHelper : ICrypto
    {
        public string Message { get; set; }
        public string CryptedMessage { get; set; }

        public ConfigurationBuilder GetConfigurationBuilder()
        {
            return new ConfigurationBuilder().WithCrypting().WithAes(new RijndaelBuilder())
                .Build()
                .WithCrypting()
#pragma warning disable SCS0011 // CBC mode is weak
                .WithAes(new RijndaelBuilder("A9@#d56P", "7§hg!8@g", "01tyç°@#78gh_aax", System.Security.Cryptography.CipherMode.CBC, System.Security.Cryptography.PaddingMode.Zeros))
                .Build(Installation.Inst00);
#pragma warning restore SCS0011 // CBC mode is weak
        }
    }
}
