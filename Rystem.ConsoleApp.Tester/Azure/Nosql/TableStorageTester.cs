using Rystem;
using Rystem.NoSql;
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public async Task DoWorkAsync(Action<object> action, UnitTestMetrics metrics, params string[] args)
        {
            string name = $"Alto{metrics.ThreadId}";
            Solute example = new Example()
            {
                PartitionKey = name,
                RowKey = "aaaaa",
                Alo = "ddd",
                Lazlo = new Lazlo() { A = 2 }
            };
            foreach(var t in await new Example().ToListAsync())
            {
                await t.DeleteAsync();
            }
            foreach (var t in await new Caruni().ToListAsync())
            {
                await t.DeleteAsync();
            }
            try
            {
                await example.DeleteAsync();
            }
            catch { }
            metrics.CheckIfNotOkExit(!await example.UpdateAsync());
            IEnumerable<Solute> examples = await example.GetAsync(x => x.PartitionKey == name && x.Timestamp >= new DateTime(1970, 1, 1) && x.Alo == "ddd");
            metrics.CheckIfNotOkExit(examples.Count() != 1);
            metrics.CheckIfNotOkExit(!await example.ExistsAsync());
            metrics.CheckIfNotOkExit(!await example.DeleteAsync());
            metrics.CheckIfNotOkExit(await example.ExistsAsync());
            examples = await example.GetAsync(x => x.PartitionKey.GreaterThan("A") && x.PartitionKey == name && x.Alo == "ddd");
            metrics.CheckIfNotOkExit(examples.Count() != 0);
            try
            {
                await example.DeleteAsync(Installation.Inst00);
            }
            catch { }
            metrics.CheckIfNotOkExit(!await example.UpdateAsync(Installation.Inst00));
            examples = await example.GetAsync(x => x.Timestamp >= new DateTime(1970, 1, 1) && x.PartitionKey == name && x.Alo == "ddd", installation: Installation.Inst00);
            //IEnumerable<Example> examples = example.Get(x => x.PartitionKey.GreaterThan("A"));
            metrics.CheckIfNotOkExit(examples.Count() != 1);
            metrics.CheckIfNotOkExit(!await example.ExistsAsync(Installation.Inst00));
            metrics.CheckIfNotOkExit(!await example.DeleteAsync(Installation.Inst00));
            metrics.CheckIfNotOkExit(await example.ExistsAsync(Installation.Inst00));
            examples = await example.GetAsync(x => x.PartitionKey == name && x.PartitionKey.GreaterThan("A") && x.Alo == "ddd", installation: Installation.Inst00);
            metrics.CheckIfNotOkExit(examples.Count() != 0);

            List<Solute> examplesForBatch = new List<Solute>();
            for (int i = 0; i < 200; i++)
                examplesForBatch.Add(new Example() { PartitionKey = $"A{metrics.ThreadId}", RowKey = i.ToString() });
            for (int i = 0; i < 105; i++)
                examplesForBatch.Add(new Caruni() { PartitionKey = $"B{metrics.ThreadId}", RowKey = i.ToString() });
            for (int i = 0; i < 100; i++)
                examplesForBatch.Add(new Example() { PartitionKey = $"B{metrics.ThreadId}", RowKey = i.ToString() });
            metrics.CheckIfNotOkExit(!examplesForBatch.UpdateBatch());
            var tt = await new Example().ToListAsync(x => x.PartitionKey == $"A{metrics.ThreadId}" || x.PartitionKey == $"B{metrics.ThreadId}");
            metrics.CheckIfNotOkExit(tt.Count != 300);
            metrics.CheckIfNotOkExit((await new Caruni().ToListAsync(x => x.PartitionKey == $"B{metrics.ThreadId}")).Count != 105);
            metrics.CheckIfNotOkExit(!await examplesForBatch.DeleteBatchAsync());
            metrics.CheckIfNotOkExit((await new Example().ToListAsync(x => x.PartitionKey == $"B{metrics.ThreadId}")).Count != 0);
            metrics.CheckIfNotOkExit((await new Caruni().ToListAsync(x => x.PartitionKey == $"A{metrics.ThreadId}")).Count != 0);
        }
        private abstract class Solute : TableStorage
        {
            public string Alo { get; set; }
        }
        private class Example : Solute
        {
            public Lazlo Lazlo { get; set; }

            public override ConfigurationBuilder GetConfigurationBuilder()
            {
                return new ConfigurationBuilder()
                    .WithNoSql(KeyManager.Instance.Storage).WithTableStorage(new TableStorageBuilder("Example"))
                    .Build(Installation.Default)
                    .WithNoSql(KeyManager.Instance.Storage)
                    .WithTableStorage(new TableStorageBuilder("Doppelganger")).Build(Installation.Inst00);
            }
        }
        private class Caruni : Solute
        {
            public Lazlo Lazlo { get; set; }
            public override ConfigurationBuilder GetConfigurationBuilder()
            {
                return new ConfigurationBuilder()
                    .WithNoSql(KeyManager.Instance.Storage).WithTableStorage(new TableStorageBuilder("Caruni"))
                    .Build(Installation.Default)
                    .WithNoSql(KeyManager.Instance.Storage)
                    .WithTableStorage(new TableStorageBuilder("Doppelganger2"))
                    .Build(Installation.Inst00);
            }
        }
        public class Lazlo
        {
            public int A { get; set; }
        }
    }
}
