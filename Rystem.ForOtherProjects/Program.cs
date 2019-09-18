using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Rystem.Azure.Storage;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Rystem.ForOtherProjects
{
    class Program
    {
        private const long MaxSize = 50000;
        private const long BaseByte = 512;
        static void Main(string[] args)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=dacanctestfuture2you;AccountKey=ULzYzxLcVQibx7QXxC4iKKJ9E02omuGg812AjLiaqBqLADkYpthFrGmgGxAlxIM+K67qhqhZszM8JylDBhR4hw==;EndpointSuffix=core.windows.net");
            CloudBlobClient Client = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer context = Client.GetContainerReference("sale");
            //bool ali = context.Exists();
            //new TrafficCardBlobLog().Save();
            //Parallel.For(0, MaxSize, async (i) =>
            // {
            //     Test50000 test50000 = new Test50000() { A = (int)i };
            //     test50000.Save(test50000.GetPartial(), MaxSize * BaseByte + BaseByte);
            //     await Task.Delay(Rystem.Utility.Alea.GetNumber(10));
            // });
            //for (int i = 0; i < MaxSize; i++)
            //{
            //    Test50000 test50000 = new Test50000() { A = (int)i };
            //    test50000.Save(test50000.GetPartial(), MaxSize * BaseByte + BaseByte);
            //}
            TestDls2();
        }
        public class ConnectionContext
        {
            private Connection Connection;
            private string ContainerName;
            public ConnectionContext(Connection connection, string containerName)
            {
                this.Connection = connection;
                this.ContainerName = containerName;
            }
            private string MakeRequest(string verb, int bodylength, QuerystringValue querystring)
            {
                string uri = $"https://{this.ContainerName}@{this.Connection.AccountName}.dfs.core.windows.net/?{querystring}";
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
                this.Connection.AddHeader(httpWebRequest, verb, bodylength, this.ContainerName);
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                {
                    return streamReader.ReadToEnd();
                }
            }
            public void Create()
            {
                MakeRequest("PUT", 0, new QuerystringValue() { Resource = "filesystem" });
            }
            private class QuerystringValue
            {
                public string Resource { get; set; } = "filesystem";
                public bool Recursive { get; set; }
                public int Timeout { get; set; }
                public override string ToString()
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append($"resource={this.Resource}");
                    if (Recursive)
                        stringBuilder.Append("&recursive=true");
                    if (Timeout > 0)
                        stringBuilder.Append($"&timeout={this.Timeout}");
                    return stringBuilder.ToString();
                }
            }
        }
        public class Connection
        {
            private const string BaseStringForConnection = "{0}\n\n\n\n{1}\n\n\n\n\n\n\n\nx-ms-date:{2}\nx-ms-version:{7}\n/{3}/{4}\nrestype:{5}\ntimeout:{6}";
            public DateTime Now => DateTime.UtcNow;
            public string AccountName { get; }
            public string AccountKey { get; }
            public string RestType { get; }
            public int Timeout { get; }
            public string Version => "2018-11-09";
            public Connection(string accountName, string accountKey, string restType = "container", int timeout = 30)
            {
                this.AccountName = accountName;
                this.AccountKey = accountKey;
                this.RestType = restType;
                this.Timeout = timeout;
            }

            private string ToToken(string verb, int bodylength, string containerName) => string.Format(BaseStringForConnection, verb, bodylength, this.NowToString(), this.AccountName, containerName, this.RestType, this.Timeout, this.Version);
            private string NowToString() => this.Now.ToString("R");
            private string AuthToString(string verb, int bodylength, string containerName) => $"Authorization:SharedKey {containerName}:{this.Signature(verb, bodylength, containerName)}";
            private string Signature(string verb, int bodylength, string containerName) => Convert.ToBase64String(CalcHMACSHA256Hash(UTF8Encoding.UTF8.GetBytes(this.ToToken(verb, bodylength, containerName)), Convert.FromBase64String(this.AccountKey)));
            private static byte[] CalcHMACSHA256Hash(byte[] plaintext, byte[] keyBase64) => new HMACSHA256(keyBase64).ComputeHash(plaintext);
            public void AddHeader(HttpWebRequest httpWebRequest, string verb, int bodylength, string containerName)
            {
                httpWebRequest.Method = verb;
                if (bodylength > 0)
                    httpWebRequest.ContentLength = bodylength;
                httpWebRequest.Headers.Add("x-ms-date:" + this.NowToString());
                httpWebRequest.Headers.Add("x-ms-version:" + this.Version);
                httpWebRequest.Headers.Add(this.AuthToString(verb, bodylength, containerName));
            }
        }

        static void TestDls2()
        {

            Connection connection = new Connection("datalakestorev2test", "i8AtmSu64rSM4TFezeQOUZMkDWTfFD3jlcVFv5rluItWvsJKl2Iu4FBvzao2PGcrT49OUh+XVz3ctfvv3IePfw==");
            ConnectionContext context = new ConnectionContext(connection, "basili");
            context.Create();
        }
    }
    public class Test50000 : IBlobStorage
    {
        private static long Maximum = TimeSpan.FromHours(1).Ticks / 50000;
        public const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=dacanctestfuture2you;AccountKey=ULzYzxLcVQibx7QXxC4iKKJ9E02omuGg812AjLiaqBqLADkYpthFrGmgGxAlxIM+K67qhqhZszM8JylDBhR4hw==;EndpointSuffix=core.windows.net";
        public BlobProperties BlobProperties { get; set; }
        public string Name { get; set; } = "test3.csv";
        public int A { get; set; }
        public long GetPartial()
        {
            DateTime utcNow = DateTime.UtcNow;
            long now = utcNow.Ticks;
            long thisStartHour = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, 0, 0).Ticks;
            return (now - thisStartHour) / Maximum;
        }
        static Test50000()
        {
            BlobStorageInstaller.Configure<Test50000>(ConnectionString, BlobStorageType.PageBlob, "aggregatedrawstats", new CsvBlobManager());
        }
    }
    public class TrafficCardBlobLog : IBlobStorage
    {
        public string CardId { get; }
        public string CustomerId { get; }
        public int ServiceId { get; }
        public string ExternalServiceId { get; }
        public int FlowId { get; }
        public string PublisherChannel { get; }
        public string PublisherTransactionId { get; }
        public int SubjectId { get; }
        public int CarrierId { get; }
        public int IntegrationId { get; }
        public long ExpirationDate { get; }
        public long FlowStartTime { get; }
        public int AttemptPosition { get; }
        public int RetryBillingAttempt { get; }
        public decimal BillingAttempt { get; }
        public string Message { get; }
        public string TransactionId { get; }
        public string ExternalAuthId { get; }
        public long ExecTime { get; }
        public string Token { get; }
        static TrafficCardBlobLog()
        {
            BlobStorageInstaller.Configure<TrafficCardBlobLog>("DefaultEndpointsProtocol=https;AccountName=wondalogs;AccountKey=6skUgScxIkiK1cCR0cmXGtydh9mAsaczuYtFNSQs9XBrnyUb7t0jjpbiU/5SzNsY2Hctbtxr97X15E1LCAtT7w==;EndpointSuffix=core.windows.net", BlobStorageType.AppendBlob, "aggregatedrawstats", new CsvBlobManager());
        }
        public TrafficCardBlobLog() { }
        public BlobProperties BlobProperties { get; set; }
        public string Name { get; set; } = "20190911/Bill.csv";
    }
}
