using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.Utility
{
    public class RetryTest : IUnitTest
    {
        public async Task<bool> DoWorkAsync(Action<object> action, params string[] args)
        {
            List<Task<string>> warps = new List<Task<string>>();
            for (int i = 0; i < 300; i++)
            {
                warps.Add(Retry.Create<string>(SetError, 2)
                   .CatchError(OnError)
                       .WithCircuitBreak(100, TimeSpan.FromSeconds(30), "MyBigIdea", OnCircuitBreakerLock)
                           .ExecuteAsync());
            }
            await Task.WhenAll(warps);
            await Task.Delay(30000);
            for (int i = 0; i < 1000; i++)
            {
                string response = await Retry.Create<string>(MakeRequest, 2)
                    .CatchError(OnError)
                        .WithCircuitBreak(200, TimeSpan.FromSeconds(30), "MyBigIdea", OnCircuitBreakerLock)
                            .ExecuteAsync();
            }
            return true;
        }
        public static Task<string> SetError()
            => throw new NotImplementedException();
        public static Task<string> MakeRequest()
            => Rystem.Utility.Alea.GetNumber(1000) % 20 == 0 ? throw new Exception() : Task.FromResult("OK");
        public static Task OnError(Exception exception)
        {
            //Console.WriteLine(exception.ToString());
            return Task.CompletedTask;
        }
        public static Task OnCircuitBreakerLock(CircuitBreakerLock circuitBreakerLock)
        {
            Console.WriteLine(circuitBreakerLock.ToString());
            return Task.CompletedTask;
        }
    }
}
