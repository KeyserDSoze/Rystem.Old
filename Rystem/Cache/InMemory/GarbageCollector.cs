using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rystem.Cache
{
    internal class GarbageCollector
    {
        private readonly List<ConcurrentDictionary<string, IInMemoryInstance>> InMemoryInstances = new List<ConcurrentDictionary<string, IInMemoryInstance>>();
        private GarbageCollector()
        {
            Thread thread = new Thread(this.Collect)
            {
                Priority = ThreadPriority.Lowest
            };
            thread.Start();
        }
        private async void Collect()
        {
            do
            {
                await Task.Delay(10000);
                try
                {
                    foreach (var instances in this.InMemoryInstances)
                    {
                        var keys = instances.Where(x => x.Value.ExpiringTime < DateTime.UtcNow.Ticks).Select(x => x.Key).ToList();
                        foreach (string key in keys)
                            instances.TryRemove(key, out _);
                    }
                }
                catch
                {
                }
            } while (true);
        }
        public static GarbageCollector Instance { get; } = new GarbageCollector();
        public void AddDictionary(ConcurrentDictionary<string, IInMemoryInstance> instances)
            => this.InMemoryInstances.Add(instances);
    }
}
