using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rystem.ZUnitTest
{
    public class UnitTestMachine<T> : IUnitTestMachine
    {
        private readonly IList<IUnitTest> Tests = new List<IUnitTest>();
        public List<TestResume> Resumes { get; } = new List<TestResume>();
        public UnitTestMachine()
        {
            this.Tests = (Assembly.GetAssembly(typeof(T)) ?? Assembly.GetAssembly(this.GetType())).GetTypes()
                .Where(x => typeof(IUnitTest).IsAssignableFrom(x) && !x.IsAbstract && !x.IsInterface)
                .OrderBy(x => x.FullName)
                .Select(x => Activator.CreateInstance(x) as IUnitTest)
                .ToList();
        }
        public void Start(Action<object> action = null, params string[] args)
            => this.StartAsync().ToResult();

        public async Task StartAsync(Action<object> action = null, params string[] args)
        {
            for (int i = 0; i < this.Tests.Count; i++)
            {
                Type type = this.Tests[i].GetType();
                Resumes.Add(await TestAsync(i, false, new CommandRequest("-All nosql")).NoContext());
            }
        }
        private async Task<TestResume> TestAsync(int actionNumber, bool hasLabel, CommandRequest command, Action<object> action = null, params string[] args)
        {
            TestResume resume = new TestResume(this.Tests[actionNumber].GetType().Name, command.IsVerbose);
            List<UnitTestMetrics> metricses = new List<UnitTestMetrics>();
            List<Task> actions = new List<Task>();
            for (int j = 0; j < command.NumberOfThread; j++)
            {
                var metrics = new UnitTestMetrics(j, command);
                metricses.Add(metrics);
                ThreadPool.UnsafeQueueUserWorkItem((x) =>
                {
                    actions.Add(ExecuteAsync(actionNumber, metrics));
                }, new object());
            }
            await Task.Delay(2000);
            await Task.WhenAll(actions);
            resume.Add(metricses);
            resume.Show(hasLabel);
            return resume;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private async Task ExecuteAsync(int index, UnitTestMetrics metrics)
        {
            try
            {
                await this.Tests[index].DoWorkAsync(null, metrics).NoContext();
            }
            catch (Exception ex)
            {
                if (ex.Message != UnitTestMetrics.UnitTestMessageException)
                    metrics.AddNotOk(ex.ToString());
            }
        }
    }
}
