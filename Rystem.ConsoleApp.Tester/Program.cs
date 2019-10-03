using Rystem.Interfaces.Utility.Tester;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rystem.ConsoleApp.Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            ITestMachine testMachine = new TestMachine<Program>("Rystem.ZConsoleApp.Tester");
            testMachine.Start();
        }
    }
}
