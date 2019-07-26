using Rystem.ConsoleApp.Tester;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.ZConsoleApp.Tester.Utility
{
    public class DeepCopier : ITest
    {
        public bool DoWork(string entry)
        {
            try
            {
                //Test test = new Test("Bird")
                //{
                //    Bird = new Bird(new List<Test>() { new Test("A"), new Test("B") })
                //    {
                //        X = 4
                //    },
                   
                //};
                //Test test2 = test.DeepCopy();
                //Console.WriteLine(test.Bird.Tests == test2.Bird.Tests);
                //Console.WriteLine(test.Bird.Tests == test.Bird.Tests);
                //Console.WriteLine(test2.Bird.Tests == test2.Bird.Tests);
                Dafne dafne = new Dafne()
                {
                    Dictionary = new Dictionary<string, string>() { { "A", "B" } }
                };
                Dafne dafne2 = dafne.DeepCopy();
                Console.WriteLine(dafne.Ale == dafne2.Ale);
                Console.WriteLine(dafne.Ale == dafne.Ale);
                Console.WriteLine(dafne2.Ale == dafne2.Ale);
            }
            catch (Exception er)
            {
                string ol = er.ToString();
            }
            return true;
        }
    }
    public class Dafne
    {
        public IDictionary<string, string> Dictionary { get; set; } = new Dictionary<string, string>();
        public string[] Ale { get; set; }
    }
    public class Test
    {
        private string Field;
        public string Actualy => this.Field;
        public Bird Bird { get; set; }
        public Test(string field) => Field = field;
    }
    public class Bird
    {
        public int X { get; set; }
        internal IList<Test> Tests;
        internal int Count => this.Tests.Count;
        public Bird(List<Test> tests) => Tests = tests;
    }
}
