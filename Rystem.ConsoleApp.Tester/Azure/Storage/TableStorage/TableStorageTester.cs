using Rystem.Azure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.ConsoleApp.Tester.Azure.Storage.TableStorage
{
    public enum ActionType
    {
        Refund
    }
    public class TableStorageTester : ITest
    {
        public const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=testerofficial;AccountKey=p2itSZpRnBV8i5wQFQWwsNs4d75SPTlVnqyDvi1XF/SLRgYRb8Af5l+w6HU+cFVSEnyNT8cWHvig5Yi7sZ4XkA==;EndpointSuffix=core.windows.net";
        private static List<string> msites = new List<string>()
        {
            "HotViewWIND",
            "MiniGamesWIND",
            "TanteVoglieWIND",
            "NotizieEGossipWIND",
            "TantiGiochiWIND",
            "BeautyRoomWIND",
            "MatchGamesWIND",
            "YourSexyDreamWIND",
            "YourBestGamesWIND",
            "DivertimentoTopWIND",
            "FingerGamesH3G",
            "GameSuperH3G",
            "KissTheSummerH3G",
            "GiocaInsiemeANoiH3G",
            "PlayAndMoreH3G",
            "PrettyGirlsH3G",
            "DivertimentoXLH3G",
            "EmpireOfGamesH3G",
            "MegaGiochiH3G",
            "EasyTimeH3G"
        };
        public bool DoWork(string entry)
        {
            TableStorageInstaller.ConfigureAsDefault("DefaultEndpointsProtocol=http;AccountName=centroservizi;AccountKey=esFapuE6DY7aspODT4WuUL03TZlFXuBEUtYirpb5WgO7hAsDgHaZmDkebILNdKYA859yhan1ri+J6rHfzguyiw==");
            Parallel.ForEach(msites, x =>
            {
                Execute(x);
            });
            return true;
            //Testone testone = new Testone()
            //{
            //    PartitionKey = "T4st",
            //    Optional = "dddd"
            //};
            //List<string> names = testone.ListOfTables();
            //testone.Update();
            //List<Testone> testones = new Testone().Fetch();
            //return true;
            //ALog actionLog = new UserAction()
            //{
            //    PartitionKey = "333333333",
            //    Operator = "Caldoro",
            //    Timestamp = DateTime.UtcNow,
            //    Action = ActionType.Refund.ToString(),
            //    Annotation = "ha fatto dei disguidi",
            //    ServiceID = 10
            //};

            ////actionLog.Success = false;
            ////actionLog.Update();
            ////List<ALog> logs = actionLog.Fetch(x => x.Operator == "Caldoro" && x.Timestamp < DateTime.UtcNow.AddHours(-5));
            ////string gg = "";
            //Example example = new Example()
            //{
            //    PartitionKey = "Alto",
            //    Alo = "ddd",
            //    Lazlo = new Lazlo() { A = 2 }
            //};
            //example.Update();
            //List<Example> examples = example.Fetch(x => x.Timestamp >= new DateTime(1970, 1, 1));
            //bool returned = example.Exists();
            //example.Delete();
            //examples = example.Fetch(x => x.Timestamp >= new DateTime(1970, 1, 1));
            //returned = example.Exists();
            //return true;
        }
        static void Execute(string msiteName)
        {
            try
            {
                MsiteId msiteId = new MsiteId().Fetch(x => x.RowKey == msiteName, 1).FirstOrDefault();
            }
            catch (Exception er)
            {
                string sorry = er.ToString();
            }
        }
    }
    public class Testone : ITableStorage
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTime Timestamp { get; set; }
        public string ETag { get; set; }
        public string Optional { get; set; }
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
    public class Example : ITableStorage
    {
        static Example() => TableStorageInstaller.ConfigureAsDefault(TableStorageTester.ConnectionString);
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
    public class MsiteId : ITableStorage
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTime Timestamp { get; set; }
        public string ETag { get; set; }
        public string Option { get; set; }
    }
}
