using Microsoft.OData.Edm;
using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.Utility
{
    public class TrafficLightTest : IUnitTest
    {
        public async Task DoWorkAsync(Action<object> action, UnitTestMetrics metrics, params string[] args)
        {
            DateTime startTime = DateTime.UtcNow;
            List<int> xx = new List<int>();
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < 20; i++)
                tasks.Add(Consob(i, xx));
            await Task.WhenAll(tasks);
            metrics.CheckIfOkExit(xx.Count == 20, xx.Count);
            metrics.CheckIfOkExit(DateTime.UtcNow.Subtract(startTime).TotalSeconds < 22, DateTime.UtcNow.Subtract(startTime).TotalSeconds);
        }
        private static readonly TrafficLight TrafficLight = new TrafficLight();
        private static async Task Consob(int x, List<int> xx)
        {
            using (var trafficLight = await TrafficLight.CreateAsync())
            {
                await Task.Delay(1000);
                xx.Add(x);
            }
        }
    }
}