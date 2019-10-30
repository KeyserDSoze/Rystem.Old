using Rystem.Crypting;
using Rystem.Enums;
using Rystem.Interfaces.Utility.Tester;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.ZConsoleApp.Tester.Utility
{
    public class AesEncryptor : ITest
    {
        public bool DoWork(Action<object> action, params string[] args)
        {
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
