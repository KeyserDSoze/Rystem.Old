using Rystem.Azure.Storage;
using Rystem.ConsoleApp.Tester;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rystem.ZConsoleApp.Tester.Azure.Storage.BlobStorageTest
{
    public class BlobStorageTest : ITest
    {
        public bool DoWork(string entry)
        {
            new Calendario().List();
            return true;
        }
    }

    public class Calendario : ABlobStorage
    {
        static Calendario()
        {
            BlobStorageInstaller.ConfigureAsDefault(StorageConnectionString, BlobStorageType.BlockBlob, nameof(Calendario).ToLower(), new JsonBlobManager());
        }
        public const string StorageConnectionString = "DefaultEndpointsProtocol=http;AccountName=colacalcio;AccountKey=bLmB0xUBaruEbpPtS9V/SLEDK4i8qJZeRKIQL9oXxu3XaufQ/9GErOIHcPq9DKWrXgoHFukz8yGy7OybbXwsvA==";
        public int Anno { get; set; }
        public string Serie { get; set; }
        public override string Name { get; set; }
    }
}
