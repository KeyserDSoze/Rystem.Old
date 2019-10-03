using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Interfaces.Utility.Tester
{
    public interface ITestMachine
    {
        void Start(Action<object> action = null, params string[] args);
    }
}
