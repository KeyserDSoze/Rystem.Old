using Rystem.ConsoleApp.Tester;
using Rystem.Interfaces.Utility;
using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.Utility
{
    public class AssemblerType : IUnitTest
    {
        public async Task<bool> DoWorkAsync(Action<object> action, params string[] args)
        {
            await Task.Delay(0);
            return Assembler.Types.Count > 0;
        }
    }
}
