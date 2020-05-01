using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.Utility
{
    public class DeepCopier : IUnitTest
    {
        public async Task<bool> DoWorkAsync(Action<object> action, params string[] args)
        {
            await Task.Delay(0);

            Test test = new Test("Bird")
            {
                Bird = new Bird(new List<Test>() { new Test("A"), new Test("B") })
                {
                    X = 4
                },

            };
            Test test2 = test.DeepCopy();
            if (test.Bird.Tests == test2.Bird.Tests)
                return false;
            if (test.Bird.Tests != test.Bird.Tests)
                return false;
            if (test2.Bird.Tests != test2.Bird.Tests)
                return false;
            Dafne dafne = new Dafne()
            {
                Dictionary = new Dictionary<string, string>() { { "A", "B" } }
            };
            Dafne dafne2 = dafne.DeepCopy();
            if (dafne.Ale != dafne2.Ale)
                return false;
            if (dafne.Dictionary == dafne2.Dictionary)
                return false;
            if (dafne.Dictionary != dafne.Dictionary)
                return false;
            if (dafne2.Dictionary != dafne2.Dictionary)
                return false;

            return true;
        }
    }
    public class Dafne
    {
        public IDictionary<string, string> Dictionary { get; set; } = new Dictionary<string, string>();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "<Pending>")]
        public string[] Ale { get; set; }
    }
    public class Test
    {
        private readonly string Field;
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
