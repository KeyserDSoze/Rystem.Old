using Newtonsoft.Json;
using Rystem.Conversion;
using Rystem.Conversion;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.ZConsoleApp.Tester.Conversion
{
    public class CsvConverter : ITest
    {
        public bool DoWork(string entry)
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
            string salo = CsvConvert.Serialize(halo);
            Halo halo3 = CsvConvert.Deserialize<Halo>(salo);
            string salo2 = JsonConvert.SerializeObject(halo3, Const.NewtonsoftConst.JsonSettings);
            Console.WriteLine(salo);
            Console.WriteLine(salo2);
            Console.WriteLine(salo.Length + " vs " + salo2.Length);
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
