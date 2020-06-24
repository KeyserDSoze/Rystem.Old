using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Rystem.UnitTest
{
    public class CommandRequest
    {
        private static readonly Regex Numbers = new Regex("[0-9]*");
        public static CommandRequest Create(IEnumerable<IUnitTest> tests, string @namespace)
        {
            Console.WriteLine("Write -exit if you want to close this app.");
            Console.WriteLine($"To test everything use -All");
            Console.WriteLine($"To test everything with a specific containing names use -All {{ContainedString}}");
            Console.WriteLine($"To test with many task at same time use -Task {{NumberOfTask}}");
            Console.WriteLine($"To test and trace all ok results use -Verbose");
            Console.WriteLine($"To test with a stress configuration (10 threads) use -Stress");
            Console.WriteLine($"To test with a value input use -Value");
            Console.WriteLine(string.Empty);
            int value = 0;
            foreach (IUnitTest test in tests)
                Console.WriteLine($"For {ToName(test, @namespace)} use -Action {value++}");
            return new CommandRequest(Console.ReadLine().ToLower());
        }
        private static string ToName(IUnitTest test, string @namespace)
            => test.GetType().FullName.Replace(@namespace, "").Trim('.');
        public bool All { get; }
        public string PatternForAll { get; }
        public int NumberOfThread { get; } = 1;
        public bool Exit { get; }
        public bool HasPattern => !string.IsNullOrWhiteSpace(this.PatternForAll);
        public int Action { get; } = -1;
        public bool HasAction => this.Action >= 0;
        public string Value { get; }
        private bool onError;
        public bool OnError
        {
            get => onError || (!this.All && this.Action < 0);
            set => onError = value;
        }
        public bool IsVerbose { get; }
        public CommandRequest(string value)
        {
            value = value.ToLower();
            if (value.Contains("-exit"))
                this.Exit = true;
            else
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(Numbers.Replace(value, string.Empty)))
                        this.Action = int.Parse(value);
                    else
                        foreach (string entry in value.Split('-').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)))
                        {
                            if (entry.StartsWith("action"))
                                this.Action = int.Parse(entry.Replace("action", string.Empty).Trim());
                            else if (entry.StartsWith("task"))
                                this.NumberOfThread = int.Parse(entry.Replace("task", string.Empty).Trim());
                            else if (entry.StartsWith("all"))
                            {
                                this.All = true;
                                this.PatternForAll = entry.Replace("all", string.Empty).Trim();
                            }
                            else if (entry.StartsWith("verbose"))
                                this.IsVerbose = true;
                            else if (entry.StartsWith("stress"))
                                this.NumberOfThread = 10;
                            else if (entry.StartsWith("value"))
                                this.Value = entry.Replace("value", string.Empty).Trim();
                            else
                                this.OnError = true;
                        }
                }
                catch
                {
                    this.OnError = true;
                }
            }
        }
    }
}
