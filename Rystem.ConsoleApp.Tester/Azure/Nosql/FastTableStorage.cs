using Rystem;
using Rystem.NoSql;
using Rystem.UnitTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rystem.Fast;

namespace Rystem.ZConsoleApp.Tester.Azure.NoSql
{
    public class FastTableStorageTester : IUnitTest
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public async Task DoWorkAsync(Action<object> action, UnitTestMetrics metrics, params string[] args)
        {
            FastInstaller.ConfigureTableStorage(KeyManager.Instance.Storage);
            string name = $"Alto{metrics.ThreadId}";
            Solute example = new Facezia()
            {
                PartitionKey = name,
                RowKey = "aaaaa",
                Alo = "ddd",
                Lazlo = new Lazlo() { A = 2 }
            };
            foreach (var t in await new Facezia().ToListAsync())
            {
                await t.DeleteAsync();
            }
            foreach (var t in await new Mondizia().ToListAsync())
            {
                await t.DeleteAsync();
            }
            await example.UpdateAsync();
            IEnumerable<Solute> examples = await example.GetAsync(x => x.PartitionKey == name && x.Timestamp >= new DateTime(1970, 1, 1) && x.Alo == "ddd");
            metrics.CheckIfNotOkExit(examples.Count() != 1);
            metrics.CheckIfNotOkExit(!await example.ExistsAsync());
            metrics.CheckIfNotOkExit(!await example.DeleteAsync());
            metrics.CheckIfNotOkExit(await example.ExistsAsync());
            examples = await example.GetAsync(x => x.PartitionKey.GreaterThan("A") && x.PartitionKey == name && x.Alo == "ddd");
            metrics.CheckIfNotOkExit(examples.Count() != 0);

            List<Solute> examplesForBatch = new List<Solute>();
            for (int i = 0; i < 200; i++)
                examplesForBatch.Add(new Facezia() { PartitionKey = $"A{metrics.ThreadId}", RowKey = i.ToString() });
            for (int i = 0; i < 105; i++)
                examplesForBatch.Add(new Mondizia() { PartitionKey = $"B{metrics.ThreadId}", RowKey = i.ToString() });
            for (int i = 0; i < 100; i++)
                examplesForBatch.Add(new Facezia() { PartitionKey = $"B{metrics.ThreadId}", RowKey = i.ToString() });
            metrics.CheckIfNotOkExit(!await examplesForBatch.UpdateBatchAsync());
            var tt = await new Facezia().ToListAsync(x => x.PartitionKey == $"A{metrics.ThreadId}" || x.PartitionKey == $"B{metrics.ThreadId}");
            metrics.CheckIfNotOkExit(tt.Count != 300);
            metrics.CheckIfNotOkExit((await new Mondizia().ToListAsync(x => x.PartitionKey == $"B{metrics.ThreadId}")).Count != 105);
            metrics.CheckIfNotOkExit(!await examplesForBatch.DeleteBatchAsync());
            metrics.CheckIfNotOkExit((await new Facezia().ToListAsync(x => x.PartitionKey == $"A{metrics.ThreadId}")).Count != 0);
            metrics.CheckIfNotOkExit((await new Mondizia().ToListAsync(x => x.PartitionKey == $"B{metrics.ThreadId}")).Count != 0);
        }
        private abstract class Solute : FastTableStorage
        {
            public string Alo { get; set; }
        }
        private class Facezia : Solute
        {
            public Lazlo Lazlo { get; set; }
        }
        private class Mondizia : Solute
        {
            public Lazlo Lazlo { get; set; }
        }
        public class Lazlo
        {
            public int A { get; set; }
        }
    }
}
