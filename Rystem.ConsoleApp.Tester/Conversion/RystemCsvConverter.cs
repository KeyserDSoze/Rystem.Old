using Newtonsoft.Json;
using Rystem.Conversion;
using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.Conversion
{
    public class RystemCsvConverter : IUnitTest
    {
        public async Task DoWorkAsync(Action<object> action, UnitTestMetrics metrics, params string[] args)
        {
            await Task.Delay(0).NoContext();
            Halo halo = new Halo()
            {
                A = "AHalo",
                B = "BHalo",
                C = 3,
                Integers = new List<int>() { 10, 11, 12 },
                Halos2 = new Dictionary<int, IHalo2>(),
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
            for (int i = 0; i < 10000; i++)
                halo.Halos2.Add(i,
                 new Halo2()
                 {
                     A = new string('"', i + 1),
                     C = i * 2,
                     D = i * 3
                 });
            string salo = halo.ToRystemCsv();
            Halo halo3 = salo.FromRystemCsv<Halo>();
            string salo2 = halo3.ToDefaultJson();
            metrics.CheckIfOkExit(salo.Length < salo2.Length);
            Model1 model1 = new Model1()
            {
                A = 4,
                Model2 = new Model2()
                {
                    A = "dddd",
                    B = "Darua"
                }
            };
            string salo3 = model1.ToRystemCsv();
            metrics.CheckIfNotOkExit(!salo3.Contains("Fregene"));
            Model3 model3 = salo3.FromRystemCsv<Model3>();
            metrics.CheckIfNotOkExit(model3.Model4.Fregene != "Darua");
        }
        private class Model1
        {
            public int A { get; set; }
            public Model2 Model2 { get; set; }
        }
        private class Model2
        {
            public string A { get; set; }
            [CsvProperty("Fregene")]
            public string B { get; set; }
        }
        private class Model3
        {
            [CsvProperty("Model2")]
            public Model4 Model4 { get; set; }
            public int A { get; set; }
        }
        private class Model4
        {
            public string Fregene { get; set; }
            public string A { get; set; }
        }
        private class Halo
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
        private interface IHalo2
        {
            string A { get; set; }
            int C { get; set; }
            int D { get; set; }
        }
        private class Halo2 : IHalo2
        {
            [CsvIgnore]
            public string A { get; set; }
            public int C { get; set; }
            public int D { get; set; }
        }
    }
}
