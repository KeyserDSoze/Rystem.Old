using System;

namespace Rystem.UnitTest
{
    public interface IUnitTestMachine
    {
        void Start(Action<object> action = null, params string[] args);
    }
}
