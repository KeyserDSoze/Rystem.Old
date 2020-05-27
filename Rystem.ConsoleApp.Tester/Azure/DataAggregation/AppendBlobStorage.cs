using Microsoft.WindowsAzure.Storage.Blob;
using Rystem.Azure.Data;
using Rystem.ConsoleApp.Tester;
using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.Azure.DataAggregation
{
    public class AppendBlobStorageTest : IUnitTest
    {
        public async Task<bool> DoWorkAsync(Action<object> action, params string[] args)
        {
            Meatball meatball = new Meatball()
            {
                Name = "Hello2.csv",
                Properties = new BlobDataProperties()
                {
                    ContentType = "text/csv"
                }
            };
            await meatball.DeleteAsync();
            meatball.A = 3;
            await meatball.WriteAsync();
            meatball.A = 5;
            await meatball.WriteAsync();
            meatball.A = 6;
            meatball.B = "dsadsadsa";
            await meatball.WriteAsync();
            IList<Meatball> meatball2 = (await meatball.ListAsync()).ToList();
            if (meatball2.Count != 3)
                return false;
            IList<DataWrapper> properties = await meatball.FetchPropertiesAsync();
            if (properties.Count != 1)
                return false;
            if (!await meatball.DeleteAsync())
                return false;
            if (await meatball.ExistsAsync())
                return false;
            meatball2 = (await meatball.ListAsync()).ToList();
            if (meatball2.Count != 0)
                return false;

            await meatball.DeleteAsync(Installation.Inst00);
            meatball.A = 3;
            await meatball.WriteAsync(0, Installation.Inst00);
            meatball.A = 5;
            await meatball.WriteAsync(0, Installation.Inst00);
            meatball.A = 6;
            meatball.B = "dsadsadsa";
            await meatball.WriteAsync(0, Installation.Inst00);
            meatball2 = (await meatball.ListAsync(installation: Installation.Inst00)).ToList();
            if (meatball2.Count != 3)
                return false;
            properties = await meatball.FetchPropertiesAsync(installation: Installation.Inst00);
            if (properties.Count != 1)
                return false;
            if (!await meatball.DeleteAsync(Installation.Inst00))
                return false;
            if (await meatball.ExistsAsync(Installation.Inst00))
                return false;
            meatball2 = (await meatball.ListAsync(installation: Installation.Inst00)).ToList();
            if (meatball2.Count != 0)
                return false;

            return true;
        }
    }
    public enum MeatballType
    {
        MyDefault = -1,
        OtherRepository
    }

    public class Meatball : IData
    {
        public ConfigurationBuilder GetConfigurationBuilder()
        {
            return new ConfigurationBuilder().WithData(StorageConnectionString)
                .WithAppendBlobStorage(new AppendBlobBuilder<Meatball>("meatball")).Build(MeatballType.MyDefault.ToInstallation())
                .WithData(StorageConnectionString)
                .WithAppendBlobStorage(new AppendBlobBuilder<Meatball>("kollipop"))
                .Build(Installation.Inst00);
        }
        public const string StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=stayhungry;AccountKey=KzdZ0SXODAR+B6/dBU0iBafWnNthOwOvrR0TUipcyFUHEAawr8h+Tl10mFTg79JQ7u2vgETC52/HYzgIXgZZpw==;EndpointSuffix=core.windows.net";
        public int A { get; set; }
        public string B { get; set; }
        public string Name { get; set; }
        public IDataProperties Properties { get; set; }
    }
}
