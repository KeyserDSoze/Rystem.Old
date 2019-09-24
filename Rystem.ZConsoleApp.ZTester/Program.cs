using Rystem.Conversion;
using Rystem.Interfaces.Conversion;
using System;
using System.Collections.Generic;

namespace Rystem.ZConsoleApp.ZTester
{
    class Program
    {
        private static readonly char[] Separator = new char[10] { '┐', '┼', '╚', '╔', '╩', '╦', '└', '┴', '┬', '├' };
        static void Main(string[] args)
        {
            Halo halo = new Halo()
            {
                A = "AHalo",
                B = "BHalo",
                C = 3,
                Halo2 = new Halo2()
                {
                    A = "AHalo2",
                    C = 45
                }
            };
            Console.WriteLine(CsvConvertV2.Serialize(halo));
        }
    }
    public class Halo
    {
        public string A { get; set; }
        [CsvIgnore]
        public string B { get; set; }
        public int C { get; set; }
        public Halo2 Halo2 { get; set; }
    }
    public class Halo2
    {
        [CsvIgnore]
        public string A { get; set; }
        public int C { get; set; }
    }
}
