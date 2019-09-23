using Rystem.Azure.NoSql;
using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.Azure.NoSql
{
    public class TableStorageTester : ITest
    {
        public const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=stayhungry;AccountKey=KzdZ0SXODAR+B6/dBU0iBafWnNthOwOvrR0TUipcyFUHEAawr8h+Tl10mFTg79JQ7u2vgETC52/HYzgIXgZZpw==;EndpointSuffix=core.windows.net";
        public bool DoWork(string entry)
        {
            Example example = new Example()
            {
                PartitionKey = "Alto",
                RowKey = "aaaaa",
                Alo = "ddd",
                Lazlo = new Lazlo() { A = 2 }
            };
            try
            {
                example.Delete();
            }
            catch { }
            if (!example.Update())
                return false;
            IEnumerable<Example> examples = example.Get(x => x.Timestamp >= new DateTime(1970, 1, 1));
            //IEnumerable<Example> examples = example.Get(x => x.PartitionKey.GreaterThan("A"));
            if (examples.Count() != 1)
                return false;
            if (!example.Exists())
                return false;
            if (!example.Delete())
                return false;
            if (example.Exists())
                return false;
            examples = example.Get(x => x.PartitionKey.GreaterThan("A"));
            if (examples.Count() != 0)
                return false;
            return true;
        }
    }
    public class Example : INoSqlStorage
    {
        static Example() => NoSqlInstaller.Configure<Example>(new NoSqlConfiguration() { ConnectionString = TableStorageTester.ConnectionString });
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTime Timestamp { get; set; }
        public string Tag { get; set; }
        public string Alo { get; set; }
        public Lazlo Lazlo { get; set; }
    }
    public class Lazlo
    {
        public int A { get; set; }
    }
}
