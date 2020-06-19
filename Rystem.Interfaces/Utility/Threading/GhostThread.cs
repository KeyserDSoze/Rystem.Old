using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Threading
{
    internal class LastRunning
    {
        public Task LastResult { get; private set; }
        private readonly Func<Task> Action;
        public LastRunning(Func<Task> action)
        {
            this.Action = action;
            this.LastResult = Task.CompletedTask;
        }
        public void Execute()
            => this.LastResult = this.Action.Invoke();
    }
    public class GhostThread
    {
        private readonly List<LastRunning> Actions = new List<LastRunning>();
        public static GhostThread Instance { get; } = new GhostThread();
        private GhostThread()
        {
            Thread thread = new Thread(this.Execute)
            {
                Priority = ThreadPriority.Lowest
            };
            thread.Start();
        }
        public void Add(Func<Task> action)
            => this.Actions.Add(new LastRunning(action));
        private async void Execute()
        {
            while (true)
            {
                for (int i = 0; i < Actions.Count; i++)
                {
                    var action = Actions[i];
                    if (action.LastResult.IsCompleted || action.LastResult.IsFaulted || action.LastResult.IsCanceled)
                        action.Execute();
                }
                await Task.Delay(120);
            }
        }
    }
}
