using Rystem.Azure.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.ConsoleApp.Tester.Azure.Storage.TableStorage
{
    public class TableStorageTester : ITest
    {
        public bool DoWork(string entry)
        {
            Example example = new Example()
            {
                PartitionKey = "Alto",
                Alo = "ddd",
                Lazlo = new Lazlo() { A = 2 }
            };
            example.Update();
            List<Example> examples = example.Fetch(x => x.Timestamp >= new DateTime(1970, 1, 1));
            bool returned = example.Exists();
            example.Delete();
            examples = example.Fetch(x => x.Timestamp >= new DateTime(1970, 1, 1));
            returned = example.Exists();
            return true;
        }
    }
    public class Example : ITableStorage
    {
        static Example() => Rystem.Azure.Storage.TableStorageInstaller.Configure<Example>("DefaultEndpointsProtocol=https;AccountName=testerofficial;AccountKey=p2itSZpRnBV8i5wQFQWwsNs4d75SPTlVnqyDvi1XF/SLRgYRb8Af5l+w6HU+cFVSEnyNT8cWHvig5Yi7sZ4XkA==;EndpointSuffix=core.windows.net");
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string ETag { get; set; }
        public string Alo { get; set; }
        public Lazlo Lazlo { get; set; }
    }
    public class Lazlo
    {
        public int A { get; set; }
    }
}
