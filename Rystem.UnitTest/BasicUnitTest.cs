using Rystem.ZConsoleApp.Tester.Cache;
using Rystem.ZUnitTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rystem.UnitTest
{
    public class BasicUnitTest
    {
        [Fact]
        public async Task TestAllAsync()
        {
            var testMachine = new UnitTestMachine<FastCacheTest>();
            await testMachine.StartAsync().NoContext();
            foreach (var test in testMachine.Resumes)
            {
                Assert.True(test.IsOk, test.Name);
            }
        }
    }
}
