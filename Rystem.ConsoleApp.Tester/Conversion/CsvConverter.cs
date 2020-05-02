using Newtonsoft.Json;
using Rystem.Conversion;
using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.Conversion
{
    public class CsvConverter : IUnitTest
    {
        public async Task<bool> DoWorkAsync(Action<object> action, params string[] args)
        {
            Halo halo = new Halo()
            {
                A = "AHalo",
                B = "BHalo",
                C = 3,
                Integers = new List<int>() { 10, 11, 12 },
                Halos2 = new Dictionary<int, IHalo2>()
                {
                    { 48, new Halo2()
                    {
                        A = "dddd",
                        C = 90,
                        D = 990
                    } },
                    { 49, new Halo2()
                    {
                        A = "dddddd",
                        C = 900,
                        D = 9900
                    } }
                },
                Halo2 = new Halo2()
                {
                    A = "AHalo2",
                    C = 45,
                    D = 4
                },
                Array = new Halo2[2]
                {
                    new Halo2()
                    {
                        A = "dddddd4",
                        C = 9001,
                        D = 99001
                    },
                    new Halo2()
                    {
                        A = "dddddd5",
                        C = 9005,
                        D = 99005
                    }
                }
            };
            string salo = halo.ToStandardCsv();
            Halo halo3 = salo.FromStandardCsv<Halo>();
            string salo2 = halo3.ToStandardJson();
            Console.WriteLine(salo);
            Console.WriteLine(salo2);
            Console.WriteLine(salo.Length + " vs " + salo2.Length);
            if (salo.Length > salo2.Length)
                return false;
            return true;
        }
    }
    public class Halo
    {
        public string A { get; set; }
        [CsvIgnore]
        public string B { get; set; }
        public int C { get; set; }
        public IHalo2 Halo2 { get; set; }
        public IList<int> Integers { get; set; }
        public IDictionary<int, IHalo2> Halos2 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Test")]
        public IHalo2[] Array { get; set; }
    }
    public interface IHalo2
    {
        string A { get; set; }
        int C { get; set; }
        int D { get; set; }
    }
    public class Halo2 : IHalo2
    {
        [CsvIgnore]
        public string A { get; set; }
        public int C { get; set; }
        public int D { get; set; }
    }
}
