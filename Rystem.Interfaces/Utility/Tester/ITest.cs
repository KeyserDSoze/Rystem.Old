using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.Interfaces.Utility.Tester
{
    public interface ITest
    {
        bool DoWork(Action<object> action, params string[] args);
    }
}
