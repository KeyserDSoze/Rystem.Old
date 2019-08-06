using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Rystem.Azure.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rystem.Azure.Storage
{
    public interface IBlobManager
    {
        /// <summary>
        /// Riferimento ai valori propri del Blob
        /// </summary>
        /// <returns></returns>
        BlobValue Value(ABlobStorage blob);
        /// <summary>
        /// Metodo che consente di effettuare delle operazioni sull'oggetto <see cref="BlobValue"/>, una volta che viene effettuata una Get su un file del Blob.
        /// </summary>
        /// <param name="blobValue"></param>
        ABlobStorage OnRetrieve(BlobValue blobValue);
    }
    public class JsonBlobManager : IBlobManager
    {
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore
        };
        public ABlobStorage OnRetrieve(BlobValue blobValue)
        {
            using (StreamReader sr = new StreamReader(blobValue.MemoryStream))
            {
                ABlobStorage blobStorage = JsonConvert.DeserializeObject<ABlobStorage>(sr.ReadToEnd(), JsonSettings);
                blobStorage.BlobProperties = blobValue.BlobProperties;
                blobStorage.Name = blobValue.DestinationFileName;
                return blobStorage;
            }
        }
        public BlobValue Value(ABlobStorage blob)
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
