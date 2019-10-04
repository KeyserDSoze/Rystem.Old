using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Rystem.Interfaces.Utility.Tester
{
    public class TestMachine<T> : ITestMachine
    {
        private IList<ITest> Tests = new List<ITest>();
        private string Namespace;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="namespace">Namespace to remove to names of your ITest's implementation.</param>
        public TestMachine(string @namespace)
        {
            this.Namespace = @namespace;
            this.Tests = (Assembly.GetAssembly(typeof(T)) ?? Assembly.GetAssembly(this.GetType())).GetTypes()
                .Where(x => typeof(ITest).IsAssignableFrom(x) && !x.IsAbstract && !x.IsInterface)
                .OrderBy(x => x.FullName)
                .Select(x => Activator.CreateInstance(x) as ITest)
                .ToList();
        }
        private bool DoWork(int value, Action<object> action, params string[] args)
        {
            if (value < 0)
                value = 0;
            if (value < this.Tests.Count)
                return this.Tests[value].DoWork(action, args);
            throw new ArgumentException($"{nameof(value)} is greater than {nameof(this.Tests)}. {value} >= {this.Tests.Count}");
        }

        private string WhatDoYouWantToSeeInAction()
        {
            int value = 0;
            foreach (ITest test in this.Tests)
                Console.WriteLine($"For {ToName(test)} use {value++}");
            Console.WriteLine("Write 'exit' if you want to close this app.");
            return Console.ReadLine();
        }
        private string ToName(ITest test)
            => test.GetType().FullName.Replace(Namespace, "").Trim('.');
        private string Result;
        public void Start(Action<object> action = null, params string[] args)
        {
            while ((Result = this.WhatDoYouWantToSeeInAction()) != "exit")
            {
                try
                {
                    Console.WriteLine("Insert your value");
                    Console.WriteLine($"Result: {this.DoWork(int.Parse(Result), action, args.Concat(new string[1] { Console.ReadLine() }).ToArray())}");
                    Console.WriteLine(string.Empty);
                    Console.Write("Press any button to continue");
                    Console.ReadLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.InnerException?.Message ?? ex.Message);
                    Console.Write("Press any button to continue");
                    Console.ReadLine();
                }
            }
        }
    }
}
