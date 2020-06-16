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
        public async Task<bool> DoWorkAsync(Action<object> action, params string[] args)
        {
            Fagioli fagioli = new Fagioli();
            await fagioli.UpdateAsync(Installation.Inst01);
            return true;
        }
        public class Fagioli : INoSql
        {
            public ConfigurationBuilder GetConfigurationBuilder()
            {
                ConfigurationBuilder monthConfigurationBuilder = new ConfigurationBuilder();
                for (int i = 1; i <= 12; i++)
                    monthConfigurationBuilder.Concat(GetConfigurationBuilder(i));
                return monthConfigurationBuilder;
            }

            private static ConfigurationBuilder GetConfigurationBuilder(int month)
            {
                return new ConfigurationBuilder().WithNoSql(TableStorageTester.ConnectionString)
                    .WithTableStorage(new TableStorageBuilder($"LogBook{month}"))
                    .Build((Installation)month);
            }
        }
    }
}
