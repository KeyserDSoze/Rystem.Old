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
            ALog log = new Deactivation();
            IList<ALog> bes = log.Get(x => x.OperatorAction == "Deactivation" &&
                          (x.Timestamp >= new DateTime(2019, 9, 9) && x.Timestamp <= DateTime.UtcNow) &&
                           x.Operator == "a.bovo@vetrya.com").OrderByDescending(x => x.Timestamp).ToList();

            //IEnumerable<BlobObject> lists = new BlobObject().List(installation: Installation.Inst00);
        }
    }

}
