using Microsoft.OData.Edm;
using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.Utility
{
    public class TrafficLightTest : IUnitTest
    {
        public async Task<bool> DoWorkAsync(Action<object> action, params string[] args)
        {
            DateTime startTime = DateTime.UtcNow;
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < 20; i++)
                tasks.Add(Consob(i));
            await Task.WhenAll(tasks);
            Console.WriteLine(DateTime.UtcNow.Subtract(startTime).TotalSeconds);
            return true;
        }
        private static readonly TrafficLight TrafficLight = new TrafficLight();
        private static async Task Consob(int x)
        {
            using (var trafficLight = await TrafficLight.CreateAsync())
            {
                await Task.Delay(1000);
                Console.WriteLine(x);
            }
        }
    }
}