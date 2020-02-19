using Rystem.Azure.AggregatedData;
using Rystem.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Reporting.WindTre.Library.Base.Blob
{
    public class BlobUtility
    {
        private BlobObject blobObject = new BlobObject();
        public async Task WriteIntoBlobAsync(MemoryStream stream, string filePath, string contentType, Installation installation)
        {
            stream.Seek(0, SeekOrigin.Begin);
            blobObject.Content = stream;
            blobObject.Name = filePath;
            blobObject.Properties = new AggregatedDataProperties()
            {
                ContentType = contentType,
            };
            await blobObject.WriteAsync(0, installation);
        }

        public Dictionary<string, byte[]> DownloadFormblobAsync(Installation installation)
        {
            IEnumerable<BlobObject> fileList = blobObject.List("Output", null, installation);
            Dictionary<string, byte[]> element = new Dictionary<string, byte[]>();

            foreach (BlobObject file in fileList)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    file.Content.CopyTo(ms);
                    ms.Position = 0;
                    element.Add(file.Name, ms.ToArray());
                }
            }
            return element;
        }
    }
}
