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
    public class BlobStorageTester : IUnitTest
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public async Task DoWorkAsync(Action<object> action, UnitTestMetrics metrics, params string[] args)
        {
            string name = $"Alto{metrics.ThreadId}";
            var qu = await new Example().FirstOrDefaultAsync();
            Solute example = new Example()
            {
                Keys = new List<string> { name, "aaaaa" },
                Alo = "ddd",
                Lazlo = new Lazlo() { A = 2 }
            };
            foreach (var t in await new Example().ToListAsync())
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
            IEnumerable<Solute> examples = (await new Example().GetAsync()).Where(x => x.Alo == "ddd");
            metrics.CheckIfNotOkExit(examples.Count() != 1);
            metrics.CheckIfNotOkExit(!await example.ExistsAsync());
            metrics.CheckIfNotOkExit(!await example.DeleteAsync());
            metrics.CheckIfNotOkExit(await example.ExistsAsync());
        }
        private abstract class Solute : BlobStorage
        {
            public string Alo { get; set; }
        }
        private class Example : Solute
        {
            public Lazlo Lazlo { get; set; }

            public override ConfigurationBuilder GetConfigurationBuilder()
            {
                return new ConfigurationBuilder()
                    .WithNoSql(KeyManager.Instance.Storage).WithBlobStorage(new BlobStorageBuilder("Example"))
                    .Build(Installation.Default)
                    .WithNoSql(KeyManager.Instance.Storage)
                    .WithBlobStorage(new BlobStorageBuilder("Doppelganger")).Build(Installation.Inst00);
            }
        }
        private class Caruni : Solute
        {
            public Lazlo Lazlo { get; set; }
            public override ConfigurationBuilder GetConfigurationBuilder()
            {
                return new ConfigurationBuilder()
                    .WithNoSql(KeyManager.Instance.Storage).WithBlobStorage(new BlobStorageBuilder("Caruni"))
                    .Build(Installation.Default)
                    .WithNoSql(KeyManager.Instance.Storage)
                    .WithBlobStorage(new BlobStorageBuilder("Doppelganger2"))
                    .Build(Installation.Inst00);
            }
        }
        public class Lazlo
        {
            public int A { get; set; }
        }
    }
}
