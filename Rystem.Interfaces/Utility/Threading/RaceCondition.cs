﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace System
{
    public sealed class RaceCondition
    {
        private static readonly object Semaphore = new object();
        private bool IsLocked;
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
                    await WaitAsync();
            }
            Exception exception = default;
            if (isTheFirst && !isWaited)
            {
                exception = await action.TryCatch();
                this.IsLocked = false;
            }
            return new RaceConditionResponse(isTheFirst && !isWaited, exception != default ? new List<Exception>() { exception } : null);

            async Task WaitAsync()
            {
                while (IsLocked)
                {
                    isWaited = true;
                    await Task.Delay(120);
                }
            }
        }
    }
}