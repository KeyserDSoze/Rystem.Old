using Microsoft.WindowsAzure.Storage.Blob;
using Rystem.Azure.AggregatedData;
using Rystem.ConsoleApp.Tester;
using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.Azure.DataAggregation
{
    public class BlockBlobStorageTest : IUnitTest
    {
        public async Task<bool> DoWorkAsync(Action<object> action, params string[] args)
        {
            Meatball2 meatball = new Meatball2()
            {
                Name = "Hello2.json",
                Properties = new AggregatedDataProperties()
                {
                    ContentType = "text/json"
                }
            };
            meatball.Delete(Enums.Installation.Inst01);
            meatball.A = 3;
            meatball.Write(installation: Enums.Installation.Inst01);
            if (!meatball.Exists(Enums.Installation.Inst01))
                return false;
            meatball.A = 6;
            meatball.B = "dsadsadsa";
            meatball.Write(installation: Enums.Installation.Inst01);
            if (meatball.Fetch(Enums.Installation.Inst01).A != meatball.A)
                return false;
            IList<Meatball2> meatball2 = meatball.List(installation: Enums.Installation.Inst01).ToList();
            if (meatball2.Count != 1)
                return false;
            IList<AggregatedDataDummy> properties = meatball.FetchProperties(installation: Enums.Installation.Inst01);
            if (properties.Count != 1)
                return false;
            if (meatball2.FirstOrDefault().B != "dsadsadsa")
                return false;
            if (!meatball.Delete(Enums.Installation.Inst01))
                return false;
            if (meatball.Exists(Enums.Installation.Inst01))
                return false;
            meatball2 = meatball.List(installation: Enums.Installation.Inst01).ToList();
            if (meatball2.Count != 0)
                return false;

            meatball.Delete(Enums.Installation.Inst00);
            meatball.A = 3;
            meatball.Write(installation: Enums.Installation.Inst00);
            if (!meatball.Exists(Enums.Installation.Inst00))
                return false;
            meatball.A = 6;
            meatball.B = "dsadsadsa";
            meatball.Write(installation: Enums.Installation.Inst00);
            if (meatball.Fetch(Enums.Installation.Inst00).A != meatball.A)
                return false;
            meatball2 = meatball.List(installation: Enums.Installation.Inst00).ToList();
            if (meatball2.Count != 1)
                return false;
            properties = meatball.FetchProperties(installation: Enums.Installation.Inst00);
            if (properties.Count != 1)
                return false;
            if (meatball2.FirstOrDefault().B != "dsadsadsa")
                return false;
            if (!meatball.Delete(Enums.Installation.Inst00))
                return false;
            if (meatball.Exists(Enums.Installation.Inst00))
                return false;
            meatball2 = meatball.List(installation: Enums.Installation.Inst00).ToList();
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
                },
                Enums.Installation.Inst01
                );
            AggregatedDataInstaller.Configure(
                new AggregatedDataConfiguration<Meatball2>()
                {

                    ConnectionString = StorageConnectionString,
                    Type = AggregatedDataType.BlockBlob,
                    Name = "akrundm"
                },
                Enums.Installation.Inst00
                );
        }
        public const string StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=stayhungry;AccountKey=KzdZ0SXODAR+B6/dBU0iBafWnNthOwOvrR0TUipcyFUHEAawr8h+Tl10mFTg79JQ7u2vgETC52/HYzgIXgZZpw==;EndpointSuffix=core.windows.net";
        public int A { get; set; }
        public string B { get; set; }
        public string Name { get; set; }
        public AggregatedDataProperties Properties { get; set; }
    }
}
