using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Rystem.Utility
{
    public class Threading
    {
        public static bool ConfigureThread(int minWorker, int minIOC)
            => ThreadPool.SetMinThreads(minWorker, minIOC);
        public static bool ConfigureThread(ThreadingType type)
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
            return ConfigureThread(minWorker, minIOC);
        }
    }
}
