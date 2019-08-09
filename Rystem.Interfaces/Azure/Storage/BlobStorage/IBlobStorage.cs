using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rystem.Azure.Storage
{
    /// <summary>
    /// Classe astratta che verrà implementata da <see cref="BlobStorage{TBlob}"/>>
    /// </summary>
    public interface IBlobStorage
    {
        BlobProperties BlobProperties { get; set; }
        string Name { get; set; }
    }
}
