using Rystem.ConsoleApp.Tester;
using Rystem.Interfaces.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.ZConsoleApp.Tester.Utility
{
    public class AssemblerType : ITest
    {
        public bool DoWork(string entry)
        {
            return Assembler.Types.Count > 0;
        }
    }
}
