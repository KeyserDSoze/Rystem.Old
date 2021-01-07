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
    public class FastBlobStorageTester : IUnitTest
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public async Task DoWorkAsync(Action<object> action, UnitTestMetrics metrics, params string[] args)
        {
            FastInstaller.ConfigureBlobStorage(KeyManager.Instance.Storage);
            string name = $"Alto{metrics.ThreadId}";
            Solute example = new Facezia()
            {
                Keys = new List<string> { name, "aaaaa" },
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
            IEnumerable<Solute> examples = (await new Facezia().GetAsync()).Where(x => x.Alo == "ddd");
            metrics.CheckIfNotOkExit(examples.Count() != 1);
            metrics.CheckIfNotOkExit(!await example.ExistsAsync());
            metrics.CheckIfNotOkExit(!await example.DeleteAsync());
            metrics.CheckIfNotOkExit(await example.ExistsAsync());
            examples = (await new Facezia().GetAsync()).Where(x => x.Alo == "ddd");
            metrics.CheckIfNotOkExit(examples.Count() != 0);
        }
        private abstract class Solute : FastBlobStorage
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
