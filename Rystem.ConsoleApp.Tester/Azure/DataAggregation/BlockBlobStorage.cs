using Rystem.Data;
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
        public async Task DoWorkAsync(Action<object> action, UnitTestMetrics metrics, params string[] args)
        {
            string name = $"Salam/Hello{metrics.ThreadId}.json";
            Meatball2 meatball = new Meatball2()
            {
                Name = name,
                Properties = new DataProperties()
                {
                    ContentType = "text/json"
                }
            };
            await meatball.DeleteAsync(Installation.Inst01);
            meatball.A = 3;
            await meatball.WriteAsync(installation: Installation.Inst01);
            metrics.CheckIfNotOkExit(!await meatball.ExistsAsync(Installation.Inst01));
            meatball.A = 6;
            meatball.B = "dsadsadsa";
            await meatball.WriteAsync(installation: Installation.Inst01);
            metrics.CheckIfNotOkExit((await meatball.FetchAsync(Installation.Inst01)).A != meatball.A);
            IList<Meatball2> meatball2 = await meatball.ToListAsync(name, installation: Installation.Inst01);
            metrics.CheckIfNotOkExit(meatball2.Count != 1);
            IList<DataWrapper> properties = await meatball.FetchPropertiesAsync(name, installation: Installation.Inst01);
            metrics.CheckIfNotOkExit(properties.Count != 1);
            metrics.CheckIfNotOkExit(meatball2.FirstOrDefault().B != "dsadsadsa");
            metrics.CheckIfNotOkExit(!await meatball.DeleteAsync(Installation.Inst01));
            metrics.CheckIfNotOkExit(await meatball.ExistsAsync(Installation.Inst01));
            meatball2 = await meatball.ToListAsync(name, installation: Installation.Inst01);
            metrics.CheckIfNotOkExit(meatball2.Count != 0);
            await meatball.DeleteAsync(Installation.Inst00);
            meatball.A = 3;
            await meatball.WriteAsync(installation: Installation.Inst00);
            metrics.CheckIfNotOkExit(!await meatball.ExistsAsync(Installation.Inst00));
            meatball.A = 6;
            meatball.B = "dsadsadsa";
            await meatball.WriteAsync(installation: Installation.Inst00);
            metrics.CheckIfNotOkExit((await meatball.FetchAsync(Installation.Inst00)).A != meatball.A);
            meatball2 = await meatball.ToListAsync(name, installation: Installation.Inst00);
            metrics.CheckIfNotOkExit(meatball2.Count != 1);
            properties = await meatball.FetchPropertiesAsync(name, installation: Installation.Inst00);
            metrics.CheckIfNotOkExit(properties.Count != 1);
            metrics.CheckIfNotOkExit(meatball2.FirstOrDefault().B != "dsadsadsa");
            metrics.CheckIfNotOkExit(!await meatball.DeleteAsync(Installation.Inst00));
            metrics.CheckIfNotOkExit(await meatball.ExistsAsync(Installation.Inst00));
            metrics.CheckIfNotOkExit(await meatball.CountAsync(name, installation: Installation.Inst00) != 0);
        }
    }

    public class Meatball2 : IData
    {
        public ConfigurationBuilder GetConfigurationBuilder()
        {
            return new ConfigurationBuilder().WithData(StorageConnectionString)
                .WithBlockBlobStorage(new BlockBlobBuilder("meatball2")).Build(Installation.Inst01)
                .WithData(StorageConnectionString)
                .WithBlockBlobStorage(new BlockBlobBuilder("akrundm")).Build(Installation.Inst00);
        }
        public const string StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=stayhungry;AccountKey=KzdZ0SXODAR+B6/dBU0iBafWnNthOwOvrR0TUipcyFUHEAawr8h+Tl10mFTg79JQ7u2vgETC52/HYzgIXgZZpw==;EndpointSuffix=core.windows.net";
        public int A { get; set; }
        public string B { get; set; }
        public string Name { get; set; }
        public DataProperties Properties { get; set; }
    }
}
