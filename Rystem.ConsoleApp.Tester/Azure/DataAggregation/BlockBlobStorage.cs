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
            await meatball.DeleteAsync(Installation.Inst01);
            meatball.A = 3;
            await meatball.WriteAsync(installation:Installation.Inst01);
            if (!await meatball.ExistsAsync(Installation.Inst01))
                return false;
            meatball.A = 6;
            meatball.B = "dsadsadsa";
            await meatball.WriteAsync(installation:Installation.Inst01);
            if ((await meatball.FetchAsync(Installation.Inst01)).A != meatball.A)
                return false;
            IList<Meatball2> meatball2 = await meatball.ToListAsync(installation:Installation.Inst01);
            if (meatball2.Count != 1)
                return false;
            IList<AggregatedDataDummy> properties = await meatball.FetchPropertiesAsync(installation:Installation.Inst01);
            if (properties.Count != 1)
                return false;
            if (meatball2.FirstOrDefault().B != "dsadsadsa")
                return false;
            if (!await meatball.DeleteAsync(Installation.Inst01))
                return false;
            if (await meatball.ExistsAsync(Installation.Inst01))
                return false;
            meatball2 = await meatball.ToListAsync(installation:Installation.Inst01);
            if (meatball2.Count != 0)
                return false;

            await meatball.DeleteAsync(Installation.Inst00);
            meatball.A = 3;
            await meatball.WriteAsync(installation:Installation.Inst00);
            if (!await meatball.ExistsAsync(Installation.Inst00))
                return false;
            meatball.A = 6;
            meatball.B = "dsadsadsa";
            await meatball.WriteAsync(installation:Installation.Inst00);
            if ((await meatball.FetchAsync(Installation.Inst00)).A != meatball.A)
                return false;
            meatball2 = await meatball.ToListAsync(installation:Installation.Inst00);
            if (meatball2.Count != 1)
                return false;
            properties = await meatball.FetchPropertiesAsync(installation:Installation.Inst00);
            if (properties.Count != 1)
                return false;
            if (meatball2.FirstOrDefault().B != "dsadsadsa")
                return false;
            if (!await meatball.DeleteAsync(Installation.Inst00))
                return false;
            if (await meatball.ExistsAsync(Installation.Inst00))
                return false;
            if (await meatball.CountAsync(installation: Installation.Inst00) != 0)
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
               Installation.Inst01
                );
            AggregatedDataInstaller.Configure(
                new AggregatedDataConfiguration<Meatball2>()
                {

                    ConnectionString = StorageConnectionString,
                    Type = AggregatedDataType.BlockBlob,
                    Name = "akrundm"
                },
               Installation.Inst00
                );
        }
        public const string StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=stayhungry;AccountKey=KzdZ0SXODAR+B6/dBU0iBafWnNthOwOvrR0TUipcyFUHEAawr8h+Tl10mFTg79JQ7u2vgETC52/HYzgIXgZZpw==;EndpointSuffix=core.windows.net";
        public int A { get; set; }
        public string B { get; set; }
        public string Name { get; set; }
        public AggregatedDataProperties Properties { get; set; }
    }
}
