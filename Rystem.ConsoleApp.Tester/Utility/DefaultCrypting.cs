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
            string value = superTest.DefaultEncrypt();
            SuperTest superTest1 = value.DefaultDecrypt<SuperTest>();
            if (superTest.A != superTest1.A)
                return false;
            string value2 = superTest.DefaultEncrypt();
            if (value == value2)
                return false;
            value = superTest.DefaultHash();
            value2 = superTest.DefaultHash();
            if (value != value2)
                return false;
            string value3 = value.DefaultHash();
            string value4 = value.DefaultHash();
            if (value3 != value4)
                return false;
            string colo = value.DefaultEncrypt();
            string valu = colo.DefaultDecrypt<string>();
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
