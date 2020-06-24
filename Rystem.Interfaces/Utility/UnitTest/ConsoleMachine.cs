using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Rystem.UnitTest
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
                if (i == 21)
                {
                    Type type = this.Tests[i].GetType();
                    Resumes.Add(await TestAsync(i, false, new Command("-All nosql")).NoContext());
                }
            }
        }
        private async Task<TestResume> TestAsync(int actionNumber, bool hasLabel, Command command, Action<object> action = null, params string[] args)
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
    public class ConsoleMachine<T> : IUnitTestMachine
    {
        private readonly IList<IUnitTest> Tests = new List<IUnitTest>();
        private readonly string Namespace;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="namespace">Namespace to remove to names of your ITest's implementation.</param>
        public ConsoleMachine(string @namespace)
        {
            this.Namespace = @namespace;
            this.Tests = (Assembly.GetAssembly(typeof(T)) ?? Assembly.GetAssembly(this.GetType())).GetTypes()
                .Where(x => typeof(IUnitTest).IsAssignableFrom(x) && !x.IsAbstract && !x.IsInterface)
                .OrderBy(x => x.FullName)
                .Select(x => Activator.CreateInstance(x) as IUnitTest)
                .ToList();
        }
        private async Task DoWorkAsync(int value, Action<object> action, UnitTestMetrics metrics, params string[] args)
        {
            if (value < 0)
                value = 0;
            if (value < this.Tests.Count)
                await this.Tests[value].DoWorkAsync(action, metrics, args).NoContext();
            else
                throw new ArgumentException($"{nameof(value)} is greater than {nameof(this.Tests)}. {value} >= {this.Tests.Count}");
        }
        public void Start(Action<object> action = null, params string[] args)
            => StartAsync(action, args).ToResult();
        public async Task DefaultTestAllAsync()
        {
            List<TestResume> resumes = new List<TestResume>();
            for (int i = 0; i < this.Tests.Count; i++)
            {
                Type type = this.Tests[i].GetType();
                resumes.Add(await TestAsync(i, false, new Command("-All")).NoContext());
            }
        }
        public async Task StartAsync(Action<object> action = null, params string[] args)
        {
            Command result;
            while (!(result = Command.Create(this.Tests, Namespace)).Exit)
            {
                if (result.OnError)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("Please insert a valid input");
                    Console.ResetColor();
                    Console.ReadLine();
                    continue;
                }
                if (result.All)
                {
                    List<TestResume> resumes = new List<TestResume>();
                    for (int i = 0; i < this.Tests.Count; i++)
                    {
                        Type type = this.Tests[i].GetType();
                        if (!result.HasPattern || type.FullName.ToLower().Contains(result.PatternForAll))
                            resumes.Add(await TestAsync(i, false, result, action, args).NoContext());
                    }
                    bool allIsOk = resumes.Any(x => x.IsOk);
                    Console.ForegroundColor = allIsOk ? ConsoleColor.DarkGreen : ConsoleColor.DarkRed;
                    Console.WriteLine($"End Test, Click enter to continue, all is ok: {allIsOk}");
                    Console.ResetColor();
                    Console.ReadLine();
                }
                else if (result.HasAction)
                    await TestAsync(result.Action, true, result, action, args).NoContext();
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("Please insert a valid input");
                    Console.ResetColor();
                    Console.ReadLine();
                    continue;
                }
            }
        }
        private async Task<TestResume> TestAsync(int actionNumber, bool hasLabel, Command command, Action<object> action = null, params string[] args)
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
                    actions.Add(ExecuteAsync(actionNumber, command.Value, metrics, action, args));
                }, new object());
            }
            await Task.Delay(2000);
            await Task.WhenAll(actions);
            resume.Add(metricses);
            resume.Show(hasLabel);
            return resume;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private async Task ExecuteAsync(int index, string input, UnitTestMetrics metrics, Action<object> action = null, params string[] args)
        {
            try
            {
                await this.DoWorkAsync(index, action, metrics, args.Concat(new string[1] { input }).ToArray()).NoContext();
            }
            catch (Exception ex)
            {
                if (ex.Message != UnitTestMetrics.UnitTestMessageException)
                    metrics.AddNotOk(ex.ToString());
            }
        }
    }
}