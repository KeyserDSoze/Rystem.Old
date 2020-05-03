using System;
using System.Threading.Tasks;

namespace Rystem.UnitTest
{
    public interface IUnitTestMachine
    {
        void Start(Action<object> action = null, params string[] args);
        Task StartAsync(Action<object> action = null, params string[] args);
    }
}
