using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Rystem.ConsoleApp.Tester
{
    class Program
    {
        static void Main()
        {
            IUnitTestMachine testMachine = new ConsoleMachine<Program>("Rystem.ZConsoleApp.Tester");
            testMachine.Start();
        }
    }
}
