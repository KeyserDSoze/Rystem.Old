using Reporting.WindTre.Library.Base.Blob;
using Rystem.Azure.AggregatedData;
using Rystem.Conversion;
using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.ZTester
{
    class Program
    {
        static List<string> xx;
        static void Main(string[] args)
        {
            //var xx = new TrafficCardLogByName().List("Billing/20191203/Ok.csv");
            var tt = new CrostaModel().List(takeCount: 10);
            var uu = "";
            //foreach(string a in xx) 
            //{
            //    string o = a;
            //}
            ////ALog log = new Deactivation();
            ////IList<ALog> bes = log.Get(x => x.OperatorAction == "Deactivation" &&
            ////              (x.Timestamp >= new DateTime(2019, 9, 9) && x.Timestamp <= DateTime.UtcNow) &&
            ////               x.Operator == "a.bovo@vetrya.com").OrderByDescending(x => x.Timestamp).ToList();
            //string resultA = GetTimedKeyForSearch2(new DateTime(2019, 10, 10, 10, 10, 10));
            //string resultB = GetTimedKeyForSearch(new DateTime(2019, 11, 10, 10, 10, 10));
            ////Console.WriteLine(resultB.LessThan(resultA));
            //Console.WriteLine(resultA);
            //Console.WriteLine(resultB);
            //Console.WriteLine(resultA.LessThan(resultB));
            //Console.WriteLine(resultA.LessThanOrEqual(resultB));
            //Console.WriteLine(resultA.GreaterThan(resultB));
            //Console.WriteLine(resultA.GreaterThanOrEqual(resultB));
            //IEnumerable<BlobObject> lists = new BlobObject().List(installation: Installation.Inst00);
        }
        public class TrafficCardLogByName : IAggregatedData
        {
            static TrafficCardLogByName()
            {
                AggregatedDataInstaller.Configure(new AggregatedDataConfiguration<TrafficCardLogByName>()
                {
                    ConnectionString = "DefaultEndpointsProtocol=https;AccountName=wondalogs;AccountKey=6skUgScxIkiK1cCR0cmXGtydh9mAsaczuYtFNSQs9XBrnyUb7t0jjpbiU/5SzNsY2Hctbtxr97X15E1LCAtT7w==;EndpointSuffix=core.windows.net",
                    Name = "rawjson",
                    Type = AggregatedDataType.AppendBlob,
                    ListReader = new TrafficCardModelListReader<TrafficCardLogByName>()
                });
            }

            public string Name { get; set; }
            public AggregatedDataProperties Properties { get; set; }

            public class TrafficCardModelListReader<TTrafficCardLog> : IAggregatedDataListReader<TTrafficCardLog> where TTrafficCardLog : IAggregatedData, new()
            {
                public async Task<IList<TTrafficCardLog>> ReadAsync(AggregatedDataDummy dummy)
                {
                    IList<TTrafficCardLog> output = new List<TTrafficCardLog>();
                    using (StreamReader streamReader = new StreamReader(dummy.Stream))
                    {
                        while (!streamReader.EndOfStream)
                        {
                            string s = await streamReader.ReadLineAsync();
                            output.Add(default);
                        }
                    }
                    return output;
                }
            }
        }
        public static void Method(DateTime dateTime = default) 
        {
            Console.WriteLine(dateTime);
        }
        private static string GetTimedKeyForSearch(DateTime date)
           => string.Format("{0:d19}~", DateTime.MaxValue.Ticks - date.Ticks);
        private static string GetTimedKeyForSearch2(DateTime date)
           => string.Format("{0:d19}{1}", DateTime.MaxValue.Ticks - date.Ticks, Guid.NewGuid().ToString("N"));
    }

}
