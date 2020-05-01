using System;
using System.Threading.Tasks;

namespace Rystem.UnitTest
{
    public interface IUnitTest
    {
        Task<bool> DoWorkAsync(Action<object> action, params string[] args);
    }
}
