using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Rystem.UnitTest
{
    public interface IUnitTest
    {
        Task DoWorkAsync(Action<object> action, UnitTestMetrics metrics, params string[] args);
    }
}