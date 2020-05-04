using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.Utility
{
    public class DefaultCrypting : IUnitTest
    {
        public async Task<bool> DoWorkAsync(Action<object> action, params string[] args)
        {
            await Task.Delay(0).ConfigureAwait(false);
            SuperTest superTest = new SuperTest { A = "Hello", B = "Hello2" };
            string value = superTest.ToAesString();
            SuperTest superTest1 = value.FromAesString<SuperTest>();
            if (superTest.A != superTest1.A)
                return false;
            string value2 = superTest.ToAesString();
            if (value == value2)
                return false;
            value = superTest.ToSha256String();
            value2 = superTest.ToSha256String();
            if (value != value2)
                return false;
            string value3 = value.ToSha256String();
            string value4 = value.ToSha256String();
            if (value3 != value4)
                return false;
            string colo = value.EncryptToStandardAes();
            string valu = colo.DecryptFromStandardAes();
            if (valu != value)
                return false;
            return true;
        }
        private class SuperTest
        {
            public string A { get; set; }
            public string B { get; set; }
        }
    }
}
