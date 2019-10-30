using Rystem.Crypting;
using Rystem.Enums;
using Rystem.Interfaces.Utility.Tester;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.ZConsoleApp.Tester.Utility
{
    public class ShaEncryptor : ITest
    {
        public bool DoWork(Action<object> action, params string[] args)
        {
            ShaHelper shaHelper = new ShaHelper()
            {
                Message = "a.rapiti@vetrya.com"
            };
            string a = shaHelper.Encrypt(Installation.Inst00).CryptedMessage;
            string b = shaHelper.Encrypt(Installation.Inst00).CryptedMessage;
            if (a != b)
                return false;
            try
            {
                shaHelper.Decrypt();
                return false;
            }
            catch
            {

            }
            return true;
        }
    }
    public class ShaHelper : ICrypto
    {
        static ShaHelper()
        {
            CryptoInstaller.Configure<ShaHelper>(new Sha256Configuration(), Installation.Inst00);
        }
        public string Message { get; set; }
        public string CryptedMessage { get; set; }
    }
}
