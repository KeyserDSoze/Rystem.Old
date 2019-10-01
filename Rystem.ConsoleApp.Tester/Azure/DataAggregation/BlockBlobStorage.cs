using Microsoft.WindowsAzure.Storage.Blob;
using Rystem.Azure.AggregatedData;
using Rystem.ConsoleApp.Tester;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem.ZConsoleApp.Tester.Azure.DataAggregation
{
    public class BlockBlobStorageTest : ITest
    {
        public bool DoWork(string entry)
        {
            Meatball2 meatball = new Meatball2()
            {
                Name = "Hello2.json",
                Properties = new AggregatedDataProperties()
                {
                    ContentType = "text/json"
                }
            };
            meatball.Delete();
            meatball.A = 3;
            meatball.Write();
            try
            {
                meatball.A = 5;
                meatball.Append();
                return false;
            }
            catch
            {
            }
            if (!meatball.Exists())
                return false;
            meatball.A = 6;
            meatball.B = "dsadsadsa";
            meatball.Write();
            if (meatball.Fetch().A != meatball.A)
                return false;
            IList<Meatball2> meatball2 = meatball.List().ToList();
            if (meatball2.Count != 1)
                return false;
            if (meatball2.FirstOrDefault().B != "dsadsadsa")
                return false;
            if (!meatball.Delete())
                return false;
            if (meatball.Exists())
                return false;
            meatball2 = meatball.List().ToList();
            if (meatball2.Count != 0)
                return false;

            meatball.Delete(Enums.Installation.Inst0);
            meatball.A = 3;
            meatball.Write(Enums.Installation.Inst0);
            try
            {
                meatball.A = 5;
                meatball.Append(0, Enums.Installation.Inst0);
                return false;
            }
            catch
            {
            }
            if (!meatball.Exists(Enums.Installation.Inst0))
                return false;
            meatball.A = 6;
            meatball.B = "dsadsadsa";
            meatball.Write(Enums.Installation.Inst0);
            if (meatball.Fetch(Enums.Installation.Inst0).A != meatball.A)
                return false;
            meatball2 = meatball.List(installation: Enums.Installation.Inst0).ToList();
            if (meatball2.Count != 1)
                return false;
            if (meatball2.FirstOrDefault().B != "dsadsadsa")
                return false;
            if (!meatball.Delete(Enums.Installation.Inst0))
                return false;
            if (meatball.Exists(Enums.Installation.Inst0))
                return false;
            meatball2 = meatball.List(installation: Enums.Installation.Inst0).ToList();
            if (meatball2.Count != 0)
                return false;

            return true;
        }
    }

    public class Meatball2 : IAggregatedData
    {
        static Meatball2()
        {
            AggregatedDataInstaller.Configure(
                new AggregatedDataConfiguration<Meatball2>()
                {

                    ConnectionString = StorageConnectionString,
                    Type = AggregatedDataType.BlockBlob
                }
                );
            AggregatedDataInstaller.Configure(
                new AggregatedDataConfiguration<Meatball2>()
                {

                    ConnectionString = StorageConnectionString,
                    Type = AggregatedDataType.BlockBlob,
                    Name = "akrundm"
                },
                Enums.Installation.Inst0
                );
        }
        public const string StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=stayhungry;AccountKey=KzdZ0SXODAR+B6/dBU0iBafWnNthOwOvrR0TUipcyFUHEAawr8h+Tl10mFTg79JQ7u2vgETC52/HYzgIXgZZpw==;EndpointSuffix=core.windows.net";
        public int A { get; set; }
        public string B { get; set; }
        public string Name { get; set; }
        public AggregatedDataProperties Properties { get; set; }
    }
}
