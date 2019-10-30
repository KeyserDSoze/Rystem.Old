using Rystem.Interfaces.Utility.Tester;
using Rystem.Utility.Crypting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.ZConsoleApp.Tester.Utility
{
    public class AesEncryptor : ITest
    {
        public bool DoWork(Action<object> action, params string[] args)
        {
            string name = "a.rapiti@vetrya.com";
            Aes.Install(System.Security.Cryptography.CipherMode.CBC, System.Security.Cryptography.PaddingMode.Zeros);
            string a = Aes.Encrypt(name);
            string b = Aes.Encrypt(name);
            if (a != b)
                return false;
            Aes.Install(System.Security.Cryptography.CipherMode.CBC, System.Security.Cryptography.PaddingMode.ISO10126);
            a = Aes.Encrypt(name);
            b = Aes.Encrypt(name);
            if (a == b)
                return false;
            return true;
        }
    }
}
