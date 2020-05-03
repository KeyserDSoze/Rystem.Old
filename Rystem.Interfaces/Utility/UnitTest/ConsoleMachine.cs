using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Rystem.UnitTest
{
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
        private async Task<bool> DoWorkAsync(int value, Action<object> action, params string[] args)
        {
            if (value < 0)
                value = 0;
            if (value < this.Tests.Count)
                return await this.Tests[value].DoWorkAsync(action, args).NoContext();
            throw new ArgumentException($"{nameof(value)} is greater than {nameof(this.Tests)}. {value} >= {this.Tests.Count}");
        }
        private string WhatDoYouWantToSeeInAction()
        {
            int value = 0;
            Console.WriteLine($"For Test everything use {AllCommand}");
            foreach (IUnitTest test in this.Tests)
                Console.WriteLine($"For {ToName(test)} use {value++}");
            Console.WriteLine("Write 'exit' if you want to close this app.");
            return Console.ReadLine().ToLower();
        }
        private string ToName(IUnitTest test)
            => test.GetType().FullName.Replace(Namespace, "").Trim('.');
        private string Result;

        private const string ExitCommand = "exit";
        private const string AllCommand = "all";
        public void Start(Action<object> action = null, params string[] args)
            => StartAsync(action, args).NoContext().GetAwaiter().GetResult();
        private static readonly Regex Numbers = new Regex("[0-9]*");
        private async Task StartAsync(Action<object> action = null, params string[] args)
        {
            while ((Result = this.WhatDoYouWantToSeeInAction()) != ExitCommand)
            {
                if (Result == AllCommand)
                {
                    List<(string, bool)> results = new List<(string, bool)>();
                    for (int i = 0; i < this.Tests.Count; i++)
                        results.Add((this.Tests[i].GetType().Name, await ExecuteAsync(i, action, string.Empty, args).NoContext()));
                    Console.WriteLine("------------------------------");
                    Console.WriteLine("Test Resume");
                    Console.WriteLine("------------------------------");
                    foreach (var t in results)
                    {
                        Console.ResetColor();
                        Console.ForegroundColor = t.Item2 ? ConsoleColor.Green : ConsoleColor.Red;
                        Console.WriteLine($"{t.Item1} --> {t.Item2}");
                        Console.ResetColor();
                    }
                }
                else if (Result != string.Empty && Numbers.Replace(Result, string.Empty) == string.Empty)
                    await ExecuteAsync(int.Parse(Result), action, null, args).NoContext();
                else
                    Console.WriteLine($"please insert a valid input");
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        private async Task<bool> ExecuteAsync(int index, Action<object> action = null, string byPassValue = null, params string[] args)
        {
            try
            {
                if (byPassValue == null)
                    Console.WriteLine("Insert your value");
                bool result = await this.DoWorkAsync(index, action, args.Concat(new string[1] { byPassValue ?? Console.ReadLine() }).ToArray()).NoContext();
                Console.WriteLine($"Result --> {result}");
                if (byPassValue == null)
                {
                    Console.WriteLine(string.Empty);
                    Console.Write("Press any button to continue");
                    Console.ReadLine();
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException?.Message ?? ex.Message);
                if (byPassValue == null)
                {
                    Console.Write("Press any button to continue");
                    Console.ReadLine();
                }
                return false;
            }
        }
    }
}
