using Rystem.Azure.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.ConsoleApp.Tester.Azure.Storage.TableStorage
{
    public enum ActionType
    {
        Refund
    }
    public class TableStorageTester : ITest
    {
        public const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=testerofficial;AccountKey=p2itSZpRnBV8i5wQFQWwsNs4d75SPTlVnqyDvi1XF/SLRgYRb8Af5l+w6HU+cFVSEnyNT8cWHvig5Yi7sZ4XkA==;EndpointSuffix=core.windows.net";
        public bool DoWork(string entry)
        {
            ALog actionLog = new UserAction()
            {
                PartitionKey = "333333333",
                Operator = "Caldoro",
                Timestamp = DateTime.UtcNow,
                Action = ActionType.Refund.ToString(),
                Annotation = "ha fatto dei disguidi",
                ServiceID = 10
            };

            //actionLog.Success = false;
            //actionLog.Update();
            //List<ALog> logs = actionLog.Fetch(x => x.Operator == "Caldoro" && x.Timestamp < DateTime.UtcNow.AddHours(-5));
            //string gg = "";
            Coso example = new Example()
            {
                PartitionKey = "Alto",
                Alo = "ddd",
                Lazlo = new Lazlo() { A = 2 }
            };
            example.Update();
            List<Coso> examples = example.Fetch(x => x.Timestamp >= new DateTime(1970, 1, 1));
            bool returned = example.Exists();
            example.Delete();
            examples = example.Fetch(x => x.Timestamp >= new DateTime(1970, 1, 1));
            returned = example.Exists();
            return true;
        }
    }
    public class UserAction : ALog
    {
        static UserAction()
        {
            TableStorageInstaller.Configure<UserAction>(TableStorageTester.ConnectionString, Rystem.Enums.Installation.Default, $"{nameof(UserAction)}Log");
        }

        public UserAction() : base()
        {
        }

        public string Action { get; set; }
        public int ServiceID { get; set; }
        public string Annotation { get; set; }
    }

    public abstract class ALog : ITableStorage
    {
        public bool Success { get; set; }
        public string Operator { get; set; }
        public string OperatorAction { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTime Timestamp { get; set; }
        public string ETag { get; set; }
    }
    public abstract class Coso : ITableStorage
    {
        static Coso() => TableStorageInstaller.ConfigureAsDefault(TableStorageTester.ConnectionString);
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTime Timestamp { get; set; }
        public string ETag { get; set; }
    }
    public class Example : Coso
    {
      
        public string Alo { get; set; }
        public Lazlo Lazlo { get; set; }
    }
    public class Lazlo
    {
        public int A { get; set; }
    }
}
