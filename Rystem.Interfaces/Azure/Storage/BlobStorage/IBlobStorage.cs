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
    public abstract class ABlobStorage
    {
        public abstract BlobProperties BlobProperties { get; set; }
        public abstract string Name { get; set; }
    }
}
