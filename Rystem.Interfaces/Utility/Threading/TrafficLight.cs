using System.Threading.Tasks;

namespace System
{
    public class TrafficLight
    {
        public bool IsLocked { get; internal set; }
        private static readonly object Semaphore = new object();
        public TrafficLight() { }
        public async Task<TrafficLightLock> CreateAsync()
        {            
            var isTheFirst = false;
            await WaitAsync();
            lock (Semaphore)
                if (!IsLocked)
                {
                    IsLocked = true;
                    isTheFirst = true;
                }
            if (!isTheFirst)
                await WaitAsync().NoContext();

            return isTheFirst ? new TrafficLightLock(this) : await this.CreateAsync().NoContext();

            async Task WaitAsync()
            {
                while (IsLocked)
                    await Task.Delay(120).NoContext();
            }
        }
    }
    public class TrafficLightLock : IDisposable
    {
        public TrafficLight TrafficLight { get; }
        public TrafficLightLock(TrafficLight trafficLight)
            => this.TrafficLight = trafficLight;
        public void Dispose()
        {
            this.TrafficLight.IsLocked = false;
        }
    }
}
