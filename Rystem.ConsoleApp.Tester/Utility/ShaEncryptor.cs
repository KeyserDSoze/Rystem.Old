using Rystem.Crypting;
using Rystem.Enums;
using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.Utility
{
    public class ShaEncryptor : IUnitTest
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public async Task<bool> DoWorkAsync(Action<object> action, params string[] args)
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
