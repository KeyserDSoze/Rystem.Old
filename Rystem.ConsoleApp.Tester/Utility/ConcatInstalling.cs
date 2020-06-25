using Rystem.NoSql;
using Rystem.UnitTest;
using Rystem.ZConsoleApp.Tester.Azure.NoSql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.ZConsoleApp.Tester.Utility
{
    public class ConcatInstalling : IUnitTest
    {
        public async Task DoWorkAsync(Action<object> action, UnitTestMetrics metrics, params string[] args)
        {
            Fagioli fagioli = new Fagioli();
            metrics.CheckIfNotOkExit(!await fagioli.UpdateAsync(Installation.Inst01));
        }
        public class Fagioli : INoSql
        {
            public string PartitionKey { get; set; }
            public string RowKey { get; set; }
            public DateTime Timestamp { get; set; }
            public string ETag { get; set; }
            public ConfigurationBuilder GetConfigurationBuilder()
            {
                ConfigurationBuilder monthConfigurationBuilder = new ConfigurationBuilder();
                for (int i = 1; i <= 12; i++)
                    monthConfigurationBuilder.Concat(GetConfigurationBuilder(i));
                return monthConfigurationBuilder;
            }

            private static ConfigurationBuilder GetConfigurationBuilder(int month)
            {
                return new ConfigurationBuilder().WithNoSql(KeyManager.Instance.Storage)
                    .WithTableStorage(new TableStorageBuilder($"LogBook{month}"))
                    .Build((Installation)month);
            }
        }
    }
}
