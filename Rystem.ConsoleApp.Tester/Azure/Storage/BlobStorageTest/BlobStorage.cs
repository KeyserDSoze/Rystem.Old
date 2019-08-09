using Microsoft.WindowsAzure.Storage.Blob;
using Rystem.Azure.Storage;
using Rystem.ConsoleApp.Tester;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.ZConsoleApp.Tester.Azure.Storage.BlobStorageTest
{
    public class BlobStorageTest : ITest
    {
        public bool DoWork(string entry)
        {
            Meatball meatball = new Meatball()
            {
                Name = "Hello2.csv",
                BlobProperties = new BlobProperties()
                {
                    ContentType = "text/csv"
                }
            };
            meatball.Delete();
            meatball.Save();
            meatball.Save();
            meatball.Save();
            Meatball meatball2 = (Meatball)meatball.Get();
            return true;
        }
    }

    public class Meatball : IBlobStorage
    {
        static Meatball()
        {
            BlobStorageInstaller.Configure<Meatball>(StorageConnectionString, BlobStorageType.AppendBlob, nameof(Meatball).ToLower(), new CsvBlobManager());
        }
        public const string StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=kynsexstorage;AccountKey=OCwrI4pGQtjc+HEfFetZ0TzExKfum2PrUfcao6cjQEyTfw1mJ15b2vNMWoBGYRkHsXwXJ/WqZXyy6BONehar+Q==;EndpointSuffix=core.windows.net";
        public int A { get; set; }
        public string B { get; set; }
        public string Name { get; set; }
        public BlobProperties BlobProperties { get; set; }
    }
}
