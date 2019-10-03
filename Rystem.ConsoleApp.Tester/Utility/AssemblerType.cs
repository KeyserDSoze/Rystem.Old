using Rystem.ConsoleApp.Tester;
using Rystem.Interfaces.Utility;
using Rystem.Interfaces.Utility.Tester;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.ZConsoleApp.Tester.Utility
{
    public class AssemblerType : ITest
    {
        public bool DoWork(Action<object> action, params string[] args)
        {
            return Assembler.Types.Count > 0;
        }
    }
}
