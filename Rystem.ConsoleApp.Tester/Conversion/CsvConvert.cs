using Rystem.Conversion;
using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.Conversion
{
    public class CsvConvert : IUnitTest
    {
        public async Task<bool> DoWorkAsync(Action<object> action, params string[] args)
        {
            await Task.Delay(0).NoContext();
            List<CsvModel> csvs = new List<CsvModel>();
            for (int i = 0; i < 100; i++)
                csvs.Add(new CsvModel()
                {
                    Name = "Ale",
                    Babel = "dsjakld,dsjakdljsa\",\"dsakdljsa\",dsadksa;dl,\",\",",
                    Hotel = i,
                    Value = 33D,
                    Nothing = 34
                });
            List<CsvModel2> csvs2 = new List<CsvModel2>();
            for (int i = 0; i < 100; i++)
                csvs2.Add(new CsvModel2()
                {
                    Name = "Alddde",
                    Babel = "dsajakld|d\"sjakdljsa,\"||\"||sakdljsa\",dsadksa;dl||.fd,",
                    Hotel = i,
                    Value = 33D,
                    Nothing = 34
                });
            string firstCsv = csvs.ToCsv();
            string secondCsv = csvs2.ToCsv('|');
            if (!firstCsv.Contains("Corto"))
                return false;
            List<CsvModel> csvsComparer = firstCsv.FromCsv<CsvModel>().ToList();
            List<CsvModel2> csvs2Comparer = secondCsv.FromCsv<CsvModel2>('|').ToList();
            for (int i = 0; i < 100; i++)
                if (csvs[i].Hotel != csvsComparer[i].Hotel)
                    return false;
            for (int i = 0; i < 100; i++)
                if (csvs2[i].Hotel != csvs2Comparer[i].Hotel)
                    return false;
            return true;
        }
        private class CsvModel
        {
            public string Name { get; set; }
            public string Babel { get; set; }
            [CsvProperty("Corto")]
            public int Hotel { get; set; }
            public double Value { get; set; }
            [CsvIgnore]
            public double Nothing { get; set; }
        }
        private class CsvModel2
        {
            public string Name { get; set; }
            public string Babel { get; set; }
            [CsvProperty("Corto")]
            public int Hotel { get; set; }
            public double Value { get; set; }
            [CsvIgnore]
            public double Nothing { get; set; }
        }
    }
}
