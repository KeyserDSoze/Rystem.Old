using Rystem.Azure.NoSql;
using Rystem.Enums;
using Rystem.Interfaces.Utility.Tester;
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public bool DoWork(Action<object> action, params string[] args)
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
            IEnumerable<Example> examples = example.Get(x => x.Timestamp >= new DateTime(1970, 1, 1) && x.Alo == "ddd");
            //IEnumerable<Example> examples = example.Get(x => x.PartitionKey.GreaterThan("A"));
            if (examples.Count() != 1)
                return false;
            if (!example.Exists())
                return false;
            if (!example.Delete())
                return false;
            if (example.Exists())
                return false;
            examples = example.Get(x => x.PartitionKey.GreaterThan("A") && x.Alo == "ddd");
            if (examples.Count() != 0)
                return false;

            try
            {
                example.Delete(Installation.Inst00);
            }
            catch { }
            if (!example.Update(Installation.Inst00))
                return false;
            examples = example.Get(x => x.Timestamp >= new DateTime(1970, 1, 1) && x.Alo == "ddd", installation: Installation.Inst00);
            //IEnumerable<Example> examples = example.Get(x => x.PartitionKey.GreaterThan("A"));
            if (examples.Count() != 1)
                return false;
            if (!example.Exists(Installation.Inst00))
                return false;
            if (!example.Delete(Installation.Inst00))
                return false;
            if (example.Exists(Installation.Inst00))
                return false;
            examples = example.Get(x => x.PartitionKey.GreaterThan("A") && x.Alo == "ddd", installation: Installation.Inst00);
            if (examples.Count() != 0)
                return false;

            List<Example> examplesForBatch = new List<Example>()
            {
                new Example(){PartitionKey = "A", RowKey="B", },
                new Example(){PartitionKey = "A", RowKey="C", },
                new Example(){PartitionKey = "A", RowKey="D", },
            };
            examplesForBatch.UpdateBatch();
            examplesForBatch = new Example().Get().ToList();
            if (examplesForBatch.Count != 3)
                return false;
            examplesForBatch.DeleteBatch();
            examplesForBatch = new Example().Get().ToList();
            if (examplesForBatch.Count != 0)
                return false;

            return true;
        }
    }
    public class Example : ITableStorage
    {
        static Example()
        {
            NoSqlInstaller.Configure<Example>(new NoSqlConfiguration() { ConnectionString = TableStorageTester.ConnectionString });
            NoSqlInstaller.Configure<Example>(new NoSqlConfiguration() { ConnectionString = TableStorageTester.ConnectionString, Name = "Doppelganger" }, Installation.Inst00);
        }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTime Timestamp { get; set; }
        public string ETag { get; set; }
        public string Alo { get; set; }
        public Lazlo Lazlo { get; set; }
    }
    public class Lazlo
    {
        public int A { get; set; }
    }
}
