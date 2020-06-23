using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.Utility
{
    public class DefaultCrypting : IUnitTest
    {
        public async Task DoWorkAsync(Action<object> action, UnitTestMetrics metrics, params string[] args)
        {
            await Task.Delay(0).ConfigureAwait(false);
            SuperTest superTest = new SuperTest { A = "Hello", B = "Hello2" };
            string value = superTest.DefaultEncrypt();
            SuperTest superTest1 = value.DefaultDecrypt<SuperTest>();
            metrics.CheckIfNotOkExit(superTest.A != superTest1.A);
            string value2 = superTest.DefaultEncrypt();
            metrics.CheckIfNotOkExit(value == value2);
            value = superTest.DefaultHash();
            value2 = superTest.DefaultHash();
            metrics.CheckIfNotOkExit(value != value2);
            string value3 = value.DefaultHash();
            string value4 = value.DefaultHash();
            metrics.CheckIfNotOkExit(value3 != value4);
            string colo = value.DefaultEncrypt();
            string valu = colo.DefaultDecrypt<string>();
            metrics.CheckIfNotOkExit(valu != value);
        }
        private class SuperTest
        {
            public string A { get; set; }
            public string B { get; set; }
        }
    }
}
