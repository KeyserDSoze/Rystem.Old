using System.Collections.Generic;
using System.Threading.Tasks;

namespace System
{
    public sealed class RaceCondition
    {
        private readonly object Semaphore = new object();
        private bool IsLocked { get; set; }
        public async Task<RaceConditionResponse> ExecuteAsync(Func<Task> action)
        {
            var isTheFirst = false;
            var isWaited = false;
            await WaitAsync();
            if (!isWaited)
            {
                lock (Semaphore)
                    if (!IsLocked)
                    {
                        IsLocked = true;
                        isTheFirst = true;
                    }
                if (!isTheFirst)
                    await WaitAsync().NoContext();
            }
            Exception exception = default;
            if (isTheFirst && !isWaited)
            {
                var result = await action.TryCatchAsync().NoContext();
                if (result.InException)
                    exception = result.Exception;
                this.IsLocked = false;
            }
            return new RaceConditionResponse(isTheFirst && !isWaited, exception != default ? new List<Exception>() { exception } : null);

            async Task WaitAsync()
            {
                while (IsLocked)
                {
                    isWaited = true;
                    await Task.Delay(120).NoContext();
                }
            }
        }
    }
}