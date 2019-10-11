using Reporting.WindTre.Library.Base.Blob;
using Rystem.Conversion;
using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.ZConsoleApp.ZTester
{
    class Program
    {
        static void Main(string[] args)
        {
            IEnumerable<BlobObject> lists = new BlobObject().List(installation: Installation.Inst00);
        }
    }
   
}
