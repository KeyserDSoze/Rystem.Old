using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem.UnitTest
{
    public class TestResume
    {
        public string Name { get; }
        public bool IsVerbose { get; }
        public TestResume(string name, bool isVerbose)
        {
            this.Name = name;
            this.IsVerbose = isVerbose;
        }
        public bool IsOk => !this.Results.Any(x => !x.IsCorrect);
        public List<UnitTestEvent> Results { get; } = new List<UnitTestEvent>();
        public void Add(IEnumerable<UnitTestMetrics> metrics)
        {
            if (this.IsVerbose)
            {
                this.Results.AddRange(metrics.SelectMany(x => x.UnitTestEvents));
            }
            else
            {
                this.Results.AddRange(metrics.SelectMany(x => x.UnitTestEvents).Where(x => !x.IsCorrect));
                var ok = metrics.SelectMany(x => x.UnitTestEvents).Where(x => x.IsCorrect).LastOrDefault();
                if (ok != null)
                    this.Results.Add(ok);
            }
        }

        public void End()
        {
            Console.WriteLine("Press enter to continue");
            Console.ReadLine();
        }
        public void Show(bool hasLabel)
        {
            if (hasLabel)
            {
                Console.WriteLine("------------------------------");
                Console.WriteLine("Test Resume");
                Console.WriteLine("------------------------------");
            }
            foreach (var t in Results)
            {
                Console.ResetColor();
                Console.ForegroundColor = t.IsCorrect ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine($"{this.Name} --> {t}");
                Console.ResetColor();
            }
            if (hasLabel)
                this.End();
        }
    }
}
