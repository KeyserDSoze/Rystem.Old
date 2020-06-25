using Rystem.Data;
using Rystem.ConsoleApp.Tester;
using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rystem.Data.Integration;

namespace Rystem.ZConsoleApp.Tester.Azure.DataAggregation
{
    public class AppendBlobStorageTest : IUnitTest
    {
        public async Task DoWorkAsync(Action<object> action, UnitTestMetrics metrics, params string[] args)
        {
            string name = $"Hello{metrics.ThreadId}.csv";
            Ricotta meatball = new Meatball()
            {
                Name = name,
                Properties = new DataProperties()
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
            await meatball.WriteAsync();
            metrics.AddOk(message: "write");
            IList<Ricotta> meatball2 = (await meatball.ListAsync(name)).ToList();
            metrics.CheckIfNotOkExit(meatball2.Count != 3, meatball2.Count);

            IList<DataWrapper> properties = await meatball.FetchPropertiesAsync(name);
            metrics.CheckIfNotOkExit(properties.Count != 1);
            metrics.CheckIfNotOkExit(!await meatball.DeleteAsync());
            metrics.CheckIfNotOkExit(await meatball.ExistsAsync());

            meatball2 = (await meatball.ListAsync(name)).ToList();
            metrics.CheckIfNotOkExit(meatball2.Count != 0);

            await meatball.DeleteAsync(Installation.Inst00);
            meatball.A = 3;
            await meatball.WriteAsync(0, Installation.Inst00);
            meatball.A = 5;
            await meatball.WriteAsync(0, Installation.Inst00);
            meatball.A = 6;
            meatball.B = "dsadsadsa";
            await meatball.WriteAsync(0, Installation.Inst00);
            meatball2 = (await meatball.ListAsync(name, installation: Installation.Inst00)).ToList();
            metrics.CheckIfNotOkExit(meatball2.Count != 3);

            properties = await meatball.FetchPropertiesAsync(name, installation: Installation.Inst00);
            metrics.CheckIfNotOkExit(properties.Count != 1);

            metrics.CheckIfNotOkExit(!await meatball.DeleteAsync(Installation.Inst00));

            metrics.CheckIfNotOkExit(await meatball.ExistsAsync(Installation.Inst00));

            meatball2 = (await meatball.ListAsync(name, installation: Installation.Inst00)).ToList();
            metrics.CheckIfNotOkExit(meatball2.Count != 0);

            try
            {
                await new ForthBall().DeleteAsync(Installation.Inst01);
                metrics.AddNotOk();
                return;
            }
            catch
            {
            }
            try
            {
                await new ZartBall().DeleteAsync(Installation.Inst02);
                metrics.AddNotOk();
                return;
            }
            catch
            {
            }
        }
    }
    public enum MeatballType
    {
        MyDefault = -1,
        OtherRepository
    }
    public class Ricotta : IData
    {
        public string Name { get; set; }
        public DataProperties Properties { get; set; }
        public int A { get; set; }
        public string B { get; set; }
        public virtual ConfigurationBuilder GetConfigurationBuilder() { return default; }
    }
    public sealed class Meatball : Ricotta
    {
        public override ConfigurationBuilder GetConfigurationBuilder()
        {
            return new ConfigurationBuilder()
                .WithData(KeyManager.Instance.Storage)
                .WithAppendBlobStorage(new AppendBlobBuilder("meatball", new JsonAvroDataManager<Ricotta>(), new JsonAvroDataManager<Ricotta>()))
                .Build(MeatballType.MyDefault.ToInstallation())
                .WithData(KeyManager.Instance.Storage)
                .WithAppendBlobStorage(new AppendBlobBuilder("kollipop"))
                .Build(Installation.Inst00);
        }
    }
    public sealed class ForthBall : Ricotta
    {
        public const string StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=stayhungry;AccountKey=KzdZ0SXODAR+B6/dBU0iBafWnNthOwOvrR0TUipcyFUHEAawr8h+Tl10mFTg79JQ7u2vgETC52/HYzgIXgZZpw==;EndpointSuffix=core.windows.net";
        public override ConfigurationBuilder GetConfigurationBuilder()
        {
            return new ConfigurationBuilder()
            .WithData(StorageConnectionString)
            .WithAppendBlobStorage(new AppendBlobBuilder("kualpop", new MeatballManager()))
            .Build(Installation.Inst01);
        }
    }
    public sealed class ZartBall : Ricotta
    {
        public const string StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=stayhungry;AccountKey=KzdZ0SXODAR+B6/dBU0iBafWnNthOwOvrR0TUipcyFUHEAawr8h+Tl10mFTg79JQ7u2vgETC52/HYzgIXgZZpw==;EndpointSuffix=core.windows.net";
        public override ConfigurationBuilder GetConfigurationBuilder()
        {
            return new ConfigurationBuilder()
            .WithData(StorageConnectionString)
            .WithAppendBlobStorage(new AppendBlobBuilder("kualpopdasdas", null, new MeatballManager()))
            .Build(Installation.Inst02);
        }
    }
    public sealed class MeatballManager : IDataReader<Meatball>, IDataWriter<Meatball>
    {
        public Task<WrapperEntity<Meatball>> ReadAsync(DataWrapper dummy)
        {
            throw new NotImplementedException();
        }
        public Task<DataWrapper> WriteAsync(Meatball entity)
        {
            throw new NotImplementedException();
        }
    }
}
