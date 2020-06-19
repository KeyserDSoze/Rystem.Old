using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rystem.Utility
{
    public class Threading
    {
        public static void StartOrchestrator(int capture, int cooldown, int scale = 5, int scaleIOC = 1, int minDifferenceFromThreadInExecutionAndThreadInPool = 20, int minDifferenceFromThreadInExecutionAndThreadInPoolIOC = 5, int min = 20, int minIOC = 8)
        {
            ThreadOrchestrator threadOrchestrator = new ThreadOrchestrator(capture, cooldown, scale, scaleIOC, minDifferenceFromThreadInExecutionAndThreadInPool, minDifferenceFromThreadInExecutionAndThreadInPoolIOC, min, minIOC);
            GhostThread.Instance.Add(() =>
            {
                threadOrchestrator.Execute(new ThreadMetric(threadOrchestrator.NumberOfEvents));
                return Task.CompletedTask;
            });
        }
        public static bool Configure(int minWorker, int minIOC)
            => ThreadPool.SetMinThreads(minWorker, minIOC);
        public static bool Configure(ThreadingType type)
        {
            ThreadPool.GetMinThreads(out int minWorker, out int minIOC);
            ThreadPool.GetMaxThreads(out int maxWorker, out int maxIOC);
            switch (type)
            {
                case ThreadingType.Proxy:
                    minWorker = maxWorker / 2;
                    minIOC = maxIOC;
                    break;
                case ThreadingType.HeavyCpu:
                    minIOC = maxIOC;
                    break;
                case ThreadingType.HeavyIO:
                    minWorker = maxWorker / 2;
                    break;
            }
            return Configure(minWorker, minIOC);
        }
    }
}
