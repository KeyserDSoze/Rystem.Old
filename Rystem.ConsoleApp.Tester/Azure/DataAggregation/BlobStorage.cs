using Microsoft.WindowsAzure.Storage.Blob;
using Rystem.Azure.AggregatedData;
using Rystem.ConsoleApp.Tester;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rystem.ZConsoleApp.Tester.Azure.NoSql.DataAggregation
{
    public class BlobStorageTest : ITest
    {
        public bool DoWork(string entry)
        {
            Meatball meatball = new Meatball()
            {
                Name = "Hello2.csv",
                Properties = new AggregatedDataProperties()
                {
                    ContentType = "text/csv"
                }
            };
            meatball.Delete();
            meatball.A = 3;
            meatball.Append();
            meatball.A = 5;
            meatball.Append();
            meatball.A = 6;
            meatball.B = "dsadsadsa";
            meatball.Append();
            IList<Meatball> meatball2 = meatball.List().ToList();
            return true;
        }
    }

    public class Meatball : IAggregatedData
    {
        static Meatball()
        {
            AggregatedDataInstaller.Configure(
                new DataAggregationConfiguration<Meatball>()
                {

                    ConnectionString = StorageConnectionString,
                    Type = AggregatedDataType.AppendBlob
                }
                );
        }
        public const string StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=stayhungry;AccountKey=KzdZ0SXODAR+B6/dBU0iBafWnNthOwOvrR0TUipcyFUHEAawr8h+Tl10mFTg79JQ7u2vgETC52/HYzgIXgZZpw==;EndpointSuffix=core.windows.net";
        public int A { get; set; }
        public string B { get; set; }
        public string Name { get; set; }
        public AggregatedDataProperties Properties { get; set; }
    }
}
