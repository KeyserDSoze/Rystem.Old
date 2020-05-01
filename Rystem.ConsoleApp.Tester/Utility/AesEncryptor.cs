using Rystem.Crypting;
using Rystem.Enums;
using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.Utility
{
    public class AesEncryptor : IUnitTest
    {
        public async Task<bool> DoWorkAsync(Action<object> action, params string[] args)
        {
            AesHelper aesHelper2 = new AesHelper() { CryptedMessage = "KTgCoh9Lxls7c6UlGorq3xnMM6b5yOt7IapwcnRbzSb8WVzCrQDNd1RAuYeWAI43fttxVS9id+wj9571c3iIclvAlOfNV50S4nO0iW+uRmpFtr4BDw/4LOB8mELaoIWtwrCwvsF8YS883VA6Sq1p6rY4XImCL9dBxLt7bwA8cU0zRdzgexhKEdbOEw8qA7o6nc7jp+Klw0QShJbwV+Q64Cn2K67Rb8ZqB4WvRkyrnuBcKKIKtInm+sEUEgpSD2/OwvDGzuzjMScGKFUksp60DQ==" };
            Console.WriteLine(aesHelper2.Decrypt().Message);
            AesHelper aesHelper = new AesHelper()
            {
                Message = "a.rapiti@vetrya.com"
            };
            string a = aesHelper.Encrypt().CryptedMessage;
            string b = aesHelper.Encrypt().CryptedMessage;
            if (a == b)
                return false;
            if (aesHelper.Decrypt().Message != "a.rapiti@vetrya.com")
                return false;
            a = aesHelper.Encrypt(Installation.Inst00).CryptedMessage;
            b = aesHelper.Encrypt(Installation.Inst00).CryptedMessage;
            if (a != b)
                return false;
            if (aesHelper.Decrypt(Installation.Inst00).Message != "a.rapiti@vetrya.com")
                return false;
            return true;
        }
    }
    public class AesHelper : ICrypto
    {
        static AesHelper()
        {
            CryptoInstaller.Configure<AesHelper>(new RjindaelConfiguration(), Installation.Default);
            CryptoInstaller.Configure<AesHelper>(new RjindaelConfiguration("A9@#d56P", "7§hg!8@g", "01tyç°@#78gh_aax", System.Security.Cryptography.CipherMode.CBC, System.Security.Cryptography.PaddingMode.Zeros), Installation.Inst00);
        }
        public string Message { get; set; }
        public string CryptedMessage { get; set; }
    }
}
