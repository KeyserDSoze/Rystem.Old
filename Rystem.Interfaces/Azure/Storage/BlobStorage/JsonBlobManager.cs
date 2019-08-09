using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rystem.Azure.Storage
{
    public class JsonBlobManager : IBlobManager
    {
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore
        };
        public IBlobStorage OnRetrieve(BlobValue blobValue, Type blobStorageType)
        {
            using (StreamReader sr = new StreamReader(blobValue.MemoryStream))
            {
                IBlobStorage blobStorage = JsonConvert.DeserializeObject<IBlobStorage>(sr.ReadToEnd(), JsonSettings);
                blobStorage.BlobProperties = blobValue.BlobProperties;
                blobStorage.Name = blobValue.DestinationFileName;
                return blobStorage;
            }
        }
        public BlobValue Value(IBlobStorage blob)
        {
            return new BlobValue()
            {
                BlobProperties = blob.BlobProperties ?? new BlobProperties() { ContentType = "text/json" },
                DestinationFileName = blob.Name,
                MemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(blob, JsonSettings)))
            };
        }
    }
}
