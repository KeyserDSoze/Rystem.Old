using Rystem.Azure.NoSql;
using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.Azure.NoSql
{
    public class TableStorageTester : IUnitTest
    {
        public const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=stayhungry;AccountKey=KzdZ0SXODAR+B6/dBU0iBafWnNthOwOvrR0TUipcyFUHEAawr8h+Tl10mFTg79JQ7u2vgETC52/HYzgIXgZZpw==;EndpointSuffix=core.windows.net";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public async Task<bool> DoWorkAsync(Action<object> action, params string[] args)
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
                await example.DeleteAsync();
            }
            catch { }
            if (!await example.UpdateAsync())
                return false;
            IEnumerable<Example> examples = await example.GetAsync(x => x.Timestamp >= new DateTime(1970, 1, 1) && x.Alo == "ddd");
            if (examples.Count() != 1)
                return false;
            if (!await example.ExistsAsync())
                return false;
            if (!await example.DeleteAsync())
                return false;
            if (await example.ExistsAsync())
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

            List<Example> examplesForBatch = new List<Example>();
            for (int i = 0; i < 200; i++)
                examplesForBatch.Add(new Example() { PartitionKey = "A", RowKey = i.ToString() });
            for (int i = 0; i < 205; i++)
                examplesForBatch.Add(new Example() { PartitionKey = "B", RowKey = i.ToString() });
            if (!examplesForBatch.UpdateBatch())
                return false;
            examplesForBatch = new Example().Get().ToList();
            if (examplesForBatch.Count != 405)
                return false;
            if (!examplesForBatch.DeleteBatch())
                return false;
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
