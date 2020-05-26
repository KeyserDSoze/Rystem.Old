using Rystem.Azure;
using Rystem.Azure.NoSql;
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
        public const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=stayhungry;AccountKey=KzdZ0SXODAR+B6/dBU0iBafWnNthOwOvrR0TUipcyFUHEAawr8h+Tl10mFTg79JQ7u2vgETC52/HYzgIXgZZpw==;EndpointSuffix=core.windows.net";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public async Task<bool> DoWorkAsync(Action<object> action, params string[] args)
        {
            FastInstaller.ConfigureTableStorage(ConnectionString);
            Solute example = new Facezia()
            {
                PartitionKey = "Alto",
                RowKey = "aaaaa",
                Alo = "ddd",
                Lazlo = new Lazlo() { A = 2 }
            };
            foreach(var t in await new Facezia().ToListAsync())
            {
                await t.DeleteAsync();
            }
            foreach (var t in await new Mondizia().ToListAsync())
            {
                await t.DeleteAsync();
            }
            await example.UpdateAsync();
            IEnumerable<Solute> examples = await example.GetAsync(x => x.Timestamp >= new DateTime(1970, 1, 1) && x.Alo == "ddd");
            if (examples.Count() != 1)
                return false;
            if (!await example.ExistsAsync())
                return false;
            if (!await example.DeleteAsync())
                return false;
            if (await example.ExistsAsync())
                return false;
            examples = await example.GetAsync(x => x.PartitionKey.GreaterThan("A") && x.Alo == "ddd");
            if (examples.Count() != 0)
                return false;

            List<Solute> examplesForBatch = new List<Solute>();
            for (int i = 0; i < 200; i++)
                examplesForBatch.Add(new Facezia() { PartitionKey = "A", RowKey = i.ToString() });
            for (int i = 0; i < 105; i++)
                examplesForBatch.Add(new Mondizia() { PartitionKey = "B", RowKey = i.ToString() });
            for (int i = 0; i < 100; i++)
                examplesForBatch.Add(new Facezia() { PartitionKey = "B", RowKey = i.ToString() });
            if (!examplesForBatch.UpdateBatch())
                return false;
            var tt = await new Facezia().ToListAsync();
            if (tt.Count != 300)
                return false;
            if ((await new Mondizia().ToListAsync()).Count != 105)
                return false;
            if (!await examplesForBatch.DeleteBatchAsync())
                return false;
            if ((await new Facezia().ToListAsync()).Count != 0)
                return false;
            if ((await new Mondizia().ToListAsync()).Count != 0)
                return false;

            return true;
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
