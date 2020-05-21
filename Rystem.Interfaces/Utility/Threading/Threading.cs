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
        public class ThreadMetric
        {
            private readonly Queue<int> Buffer = new Queue<int>();
            private readonly Queue<int> BufferIOC = new Queue<int>();
            private readonly int Max;
            public ThreadMetric(int max)
                => this.Max = max;
            public void AddDifference(int value)
            {
                this.Buffer.Enqueue(value);
                if (this.Buffer.Count > this.Max)
                    this.Buffer.Dequeue();
            }
            public void AddDifferenceIOC(int value)
            {
                this.BufferIOC.Enqueue(value);
                if (this.BufferIOC.Count > this.Max)
                    this.BufferIOC.Dequeue();
            }
            public int BufferCount => this.Buffer.Count;
            public int BufferIOCCount => this.BufferIOC.Count;
            public IEnumerable<int> GetBuffer()
                => this.Buffer.AsEnumerable();
            public IEnumerable<int> GetBufferIOC()
               => this.BufferIOC.AsEnumerable();
        }
        public class ThreadOrchestrator
        {
            private const int WaitTime = 10000;
            private readonly int Capture;
            private readonly int Cooldown;
            private readonly int MinDifferenceFromThreadInExecutionAndThreadInPool;
            private readonly int MinDifferenceFromThreadInExecutionAndThreadInPoolIOC;
            private readonly int Scale;
            private readonly int ScaleIOC;
            private readonly int Min;
            private readonly int MinIOC;
            public int NumberOfEvents { get; }
            public ThreadOrchestrator(int capture, int cooldown, int scale, int scaleIOC, int minDifferenceFromThreadInExecutionAndThreadInPool, int minDifferenceFromThreadInExecutionAndThreadInPoolIOC, int min, int minIOC)
            {
                this.Capture = capture;
                this.NumberOfEvents = this.Capture * 60000 / ThreadOrchestrator.WaitTime;
                this.Cooldown = cooldown;
                this.Scale = scale;
                this.ScaleIOC = scaleIOC;
                this.MinDifferenceFromThreadInExecutionAndThreadInPool = minDifferenceFromThreadInExecutionAndThreadInPool;
                this.MinDifferenceFromThreadInExecutionAndThreadInPoolIOC = minDifferenceFromThreadInExecutionAndThreadInPoolIOC;
                this.Min = min;
                this.MinIOC = minIOC;
            }
            public async void Execute(object x)
            {
                ThreadMetric metrics = (ThreadMetric)x;
                while (true)
                {
                    await Task.Delay(WaitTime);
                    ThreadPool.GetMinThreads(out int minWorker, out int minIOC);
                    ThreadPool.GetMaxThreads(out int maxWorker, out int maxIOC);
                    ThreadPool.GetAvailableThreads(out int availableWorker, out int availableIOC);
                    int usedWorker = maxWorker - availableWorker;
                    int usedIOC = maxIOC - availableIOC;
                    metrics.AddDifference(minWorker - usedWorker);
                    metrics.AddDifferenceIOC(minIOC - usedIOC);
                    if (metrics.BufferCount >= this.NumberOfEvents)
                    {
                        bool update = false;
                        bool updateIOC = false;
                        if (metrics.GetBuffer().All(t => t < this.MinDifferenceFromThreadInExecutionAndThreadInPool))
                        {
                            update = true;
                            minWorker += this.Scale;
                        }
                        else if (minWorker > this.Min && metrics.GetBuffer().All(t => t > this.MinDifferenceFromThreadInExecutionAndThreadInPool))
                        {
                            update = true;
                            minWorker -= this.Scale;
                        }
                        if (metrics.GetBufferIOC().All(t => t < this.MinDifferenceFromThreadInExecutionAndThreadInPoolIOC))
                        {
                            updateIOC = true;
                            minIOC += this.ScaleIOC;
                        }
                        else if (minIOC > this.MinIOC && metrics.GetBufferIOC().All(t => t > this.MinDifferenceFromThreadInExecutionAndThreadInPoolIOC))
                        {
                            updateIOC = true;
                            minIOC -= this.ScaleIOC;
                        }
                        if (update || updateIOC)
                        {
                            ThreadPool.SetMinThreads(minWorker, minIOC);
                            await Task.Delay(this.Cooldown * 60000);
                        }
                    }
                }
            }
        }
        public static void StartThreadOrchestrator(int capture, int cooldown, int scale = 5, int scaleIOC = 1, int minDifferenceFromThreadInExecutionAndThreadInPool = 20, int minDifferenceFromThreadInExecutionAndThreadInPoolIOC = 5, int min = 20, int minIOC = 8)
        {
            ThreadOrchestrator threadOrchestrator = new ThreadOrchestrator(capture, cooldown, scale, scaleIOC, minDifferenceFromThreadInExecutionAndThreadInPool, minDifferenceFromThreadInExecutionAndThreadInPoolIOC, min, minIOC);
            ThreadPool.UnsafeQueueUserWorkItem(threadOrchestrator.Execute, new ThreadMetric(threadOrchestrator.NumberOfEvents));
        }
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
