using Reporting.WindTre.Library.Base.Blob;
using Rystem.Conversion;
using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem.ZConsoleApp.ZTester
{
    class Program
    {
        static void Main(string[] args)
        {
            //ALog log = new Deactivation();
            //IList<ALog> bes = log.Get(x => x.OperatorAction == "Deactivation" &&
            //              (x.Timestamp >= new DateTime(2019, 9, 9) && x.Timestamp <= DateTime.UtcNow) &&
            //               x.Operator == "a.bovo@vetrya.com").OrderByDescending(x => x.Timestamp).ToList();
            string resultA = GetTimedKeyForSearch2(new DateTime(2019, 10, 10, 10, 10, 10));
            string resultB = GetTimedKeyForSearch(new DateTime(2019, 11, 10, 10, 10, 10));
            //Console.WriteLine(resultB.LessThan(resultA));
            Console.WriteLine(resultA);
            Console.WriteLine(resultB);
            Console.WriteLine(resultA.LessThan(resultB));
            Console.WriteLine(resultA.LessThanOrEqual(resultB));
            Console.WriteLine(resultA.GreaterThan(resultB));
            Console.WriteLine(resultA.GreaterThanOrEqual(resultB));
            //IEnumerable<BlobObject> lists = new BlobObject().List(installation: Installation.Inst00);
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
